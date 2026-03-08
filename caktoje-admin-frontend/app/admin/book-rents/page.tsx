'use client';

import { useEffect, useState } from 'react';
import axios from '../../lib/axios';
import Modal from '../../components/Modal';
import { CheckCircle, Trash2 } from 'lucide-react';
import SearchableDropdown from '../../components/SearchableDropdown';

interface BookRent {
    id: number;
    renter: {
        id: string;
        userName: string;
    };
    bookStock: {
        id: number;
        book: {
            name: string;
        };
    };
    rentedAt: string;
    expiresAt: string;
    returnedAt: string | null;
}

interface PaginatedBookRents {
    items: BookRent[];
    totalPages: number;
}

interface User {
    id: string;
    userName: string;
    firstName: string;
    lastName: string;
    phoneNumber: string;
}

interface BookStock {
    id: number;
    book: { name: string; };
    putwall: string;
    row: number;
    column: number;
}

export default function BookRentsPage() {
    const [bookRents, setBookRents] = useState<BookRent[]>([]);
    const [page, setPage] = useState(1);
    const [totalPages, setTotalPages] = useState(1);
    const [bookStockIds, setBookStockIds] = useState<number[]>([]);
    const [renterIds, setRenterIds] = useState<string[]>([]);
    const [overdue, setOverdue] = useState<boolean | null>(null);
    const [loading, setLoading] = useState(true);
    const [isModalOpen, setIsModalOpen] = useState(false);

    // Form state
    const [selectedRenter, setSelectedRenter] = useState<User | null>(null);
    const [selectedBookStockId, setSelectedBookStockId] = useState('');
    const [expiresAt, setExpiresAt] = useState('');

    const [filterByUser, setFilterByUser] = useState<User | null>(null);

    const [allUsers, setAllUsers] = useState<User[]>([]);
    const [allBookStocks, setAllBookStocks] = useState<BookStock[]>([]);

    const fetchBookRents = async () => {
        setLoading(true);
        try {
            const response = await axios.get<PaginatedBookRents>('/api/admin/book-rents', {
                params: {
                    page,
                    pageSize: 10,
                    bookStockIds: bookStockIds.join(','),
                    renterIds: filterByUser?.id,
                    overdue,
                },
            });
            setBookRents(response.data.items);
            setTotalPages(response.data.totalPages);
        } catch (error) {
            console.error('Failed to fetch book rents:', error);
        } finally {
            setLoading(false);
        }
    };

    const fetchUsersAndBookStocks = async () => {
        try {
            // Assuming you have a /users endpoint
            const [usersRes, bookStocksRes] = await Promise.all([
                axios.get('/api/admin/users', { params: { pageSize: 1000 } }),
                axios.get('/api/admin/book-stocks', { params: { pageSize: 1000 } })
            ]);
            setAllUsers(usersRes.data.items);
            setAllBookStocks(bookStocksRes.data.items);
        } catch (error) {
            console.error('Failed to fetch users or book stocks:', error);
        }
    };

    useEffect(() => {
        fetchBookRents();
    }, [page, bookStockIds, renterIds, overdue, filterByUser?.id]);

    useEffect(() => {
        fetchUsersAndBookStocks();
    }, []);

    const openModalForCreate = () => {
        setSelectedRenter(null);
        setSelectedBookStockId('');
        setExpiresAt('');
        setIsModalOpen(true);
    };

    const handleFormSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        if (!selectedRenter) return;
        const payload = {
            renterId: selectedRenter.id,
            bookStockId: selectedBookStockId,
            expiresAt,
        };
        try {
            await axios.post('/api/admin/book-rents', payload);
            setIsModalOpen(false);
            fetchBookRents();
        } catch (error) {
            console.error('Failed to create book rent:', error);
        }
    };

    const handleMarkAsReturned = async (id: number) => {
        if (window.confirm('Are you sure you want to mark this rent as returned?')) {
            try {
                await axios.put(`/api/admin/book-rents/${id}/return`);
                fetchBookRents();
            } catch (error) {
                console.error('Failed to mark as returned:', error);
            }
        }
    };

    return (
        <div className="bg-white p-6 rounded-lg shadow-md">
            <div className="flex justify-between items-center mb-6">
                <h1 className="text-2xl font-bold text-gray-800">Book Rents</h1>
                <button
                    onClick={openModalForCreate}
                    className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 transition-colors"
                >
                    Create Rent
                </button>
            </div>
            <div className='flex mb-4'>
            <div>
                <label htmlFor="renter" className="block text-sm font-medium text-gray-700">Renter</label>
                <SearchableDropdown
                    options={allUsers}
                    selected={filterByUser}
                    onChange={setFilterByUser}
                    placeholder="Search for a user..."
                    getOptionLabel={(user) => `${user.firstName} ${user.lastName} (${user.phoneNumber})`}
                />
            </div>
            <div className="ml-4">
                <div>
                <label htmlFor="overdue" className="block text-sm font-medium text-gray-700">Overdue Status</label>
                <SearchableDropdown
                    options={[{ id: 'overdue', name: 'Overdue' }, { id: 'not-overdue', name: 'Not Overdue' }]}
                    selected={overdue ? { id: 'overdue', name: 'Overdue' } : overdue === false ? { id: 'not-overdue', name: 'Not Overdue' } : null}
                    onChange={(val) => setOverdue(val ? (val.id === 'overdue' ? true : false) : null)}
                    placeholder="Select Overdue Status..."
                    getOptionLabel={(option) => option.name}
                />
            </div>
            </div>
            </div>
            {loading ? (
                <div className="text-center py-10"><p className="text-gray-500">Loading...</p></div>
            ) : (
                <div className="overflow-x-auto">
                    <table className="min-w-full bg-white">
                        <thead className="bg-gray-50">
                            <tr>
                                <th className="py-3 px-4 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Book</th>
                                <th className="py-3 px-4 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Renter</th>
                                <th className="py-3 px-4 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Rented At</th>
                                <th className="py-3 px-4 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Due Date</th>
                                <th className="py-3 px-4 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Returned At</th>
                                <th className="py-3 px-4 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Actions</th>
                            </tr>
                        </thead>
                        <tbody className="divide-y divide-gray-200">
                            {bookRents.map((rent) => (
                                <tr key={rent.id} className={`hover:bg-gray-50 ${!rent.returnedAt && new Date(rent.expiresAt) < new Date() ? 'bg-red-50' : ''}`}>
                                    <td className="py-4 px-4 whitespace-nowrap">{rent.bookStock.book.name}</td>
                                    <td className="py-4 px-4 whitespace-nowrap">{rent.renter.userName}</td>
                                    <td className="py-4 px-4 whitespace-nowrap">{new Date(rent.rentedAt).toLocaleDateString()}</td>
                                    <td className="py-4 px-4 whitespace-nowrap">{new Date(rent.expiresAt).toLocaleDateString()}</td>
                                    <td className="py-4 px-4 whitespace-nowrap">{rent.returnedAt ? new Date(rent.returnedAt).toLocaleDateString() : 'Not returned'}</td>
                                    <td className="py-4 px-4 whitespace-nowrap">
                                        <div className="flex space-x-2">
                                            {!rent.returnedAt && (
                                                <button onClick={() => handleMarkAsReturned(rent.id)} className="text-green-600 hover:text-green-800" title="Mark as Returned">
                                                    <CheckCircle size={20} />
                                                </button>
                                            )}
                                        </div>
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                </div>
            )}
            <div className="mt-6 flex justify-between items-center">
                <button
                    onClick={() => setPage(page - 1)}
                    disabled={page === 1}
                    className="px-4 py-2 bg-gray-200 text-gray-800 rounded-md disabled:opacity-50 hover:bg-gray-300"
                >
                    Previous
                </button>
                <span className="text-sm text-gray-700">Page {page} of {totalPages}</span>
                <button
                    onClick={() => setPage(page + 1)}
                    disabled={page === totalPages}
                    className="px-4 py-2 bg-gray-200 text-gray-800 rounded-md disabled:opacity-50 hover:bg-gray-300"
                >
                    Next
                </button>
            </div>

            <Modal isOpen={isModalOpen} onClose={() => setIsModalOpen(false)} title="Create Book Rent">
                <form onSubmit={handleFormSubmit}>
                    <div className="space-y-4">
                        <div>
                            <label htmlFor="renter" className="block text-sm font-medium text-gray-700">Renter</label>
                            <SearchableDropdown
                                options={allUsers}
                                selected={selectedRenter}
                                onChange={setSelectedRenter}
                                placeholder="Search for a user..."
                                getOptionLabel={(user) => `${user.firstName} ${user.lastName} (${user.phoneNumber})`}
                            />
                        </div>
                        <div>
                            <label htmlFor="bookStock" className="block text-sm font-medium text-gray-700">Book Stock</label>
                            <select id="bookStock" value={selectedBookStockId} onChange={(e) => setSelectedBookStockId(e.target.value)} className="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500" required>
                                <option value="">Select a book stock</option>
                                {allBookStocks.map(stock => <option key={stock.id} value={stock.id}>{`${stock.book.name} (Putwall: ${stock.putwall}, Row: ${stock.row}, Col: ${stock.column})`}</option>)}
                            </select>
                        </div>
                        <div>
                            <label htmlFor="expiresAt" className="block text-sm font-medium text-gray-700">Due Date</label>
                            <input type="date" id="expiresAt" value={expiresAt} onChange={(e) => setExpiresAt(e.target.value)} className="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500" required />
                        </div>
                        <div className="flex justify-end space-x-2 pt-4">
                            <button type="button" onClick={() => setIsModalOpen(false)} className="px-4 py-2 text-sm font-medium text-gray-700 bg-gray-100 rounded-md hover:bg-gray-200">Cancel</button>
                            <button type="submit" className="px-4 py-2 text-sm font-medium text-white bg-blue-600 rounded-md hover:bg-blue-700">Create</button>
                        </div>
                    </div>
                </form>
            </Modal>
        </div>
    );
}

'use client';

import { useEffect, useState } from 'react';
import axios from '../../lib/axios';
import Modal from '../../components/Modal';
import { Pencil, Trash2 } from 'lucide-react';
import SingleCustomDropdown from '../../components/SingleCustomDropdown';

interface BookStock {
  id: number;
  putwall: string;
  row: number;
  column: number;
  book: {
    id: number;
    name: string;
  };
}

interface PaginatedBookStocks {
  items: BookStock[];
  totalPages: number;
}

interface Book {
    id: number;
    name: string;
}

interface PutwallSection {
    id: number;
    row: number;
    column: number;
    putwallId: number;
    putwall: {
        id: number;
        name: string;
        rows: number;
        columns: number;
    };
}

interface Putwall {
    id: number;
    name: string;
    rows: number;
    columns: number;
}

export default function BookStocksPage() {
  const [bookStocks, setBookStocks] = useState<BookStock[]>([]);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [query, setQuery] = useState('');
  const [bookIds, setBookIds] = useState<number[]>([]);
  const [putwallSectionIds, setPutwallSectionIds] = useState<number[]>([]);
  const [loading, setLoading] = useState(true);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingBookStock, setEditingBookStock] = useState<BookStock | null>(null);

  // Form state
  const [selectedBook, setSelectedBook] = useState<Book | null>(null);
  const [selectedPutwall, setSelectedPutwall] = useState<Putwall | null>(null);
  const [selectedPutwallSection, setSelectedPutwallSection] = useState<PutwallSection | null>(null);

  const [allBooks, setAllBooks] = useState<Book[]>([]);
  const [allPutwallSections, setAllPutwallSections] = useState<PutwallSection[]>([]);
  const [allPutwalls, setAllPutwalls] = useState<Putwall[]>([]);
  const [filteredPutwallSections, setFilteredPutwallSections] = useState<PutwallSection[]>([]);


  const fetchBookStocks = async () => {
    setLoading(true);
    try {
      const response = await axios.get<PaginatedBookStocks>('/api/admin/book-stocks', {
        params: {
          query,
          page,
          pageSize: 10,
          bookIds: bookIds.join(','),
          putwallSectionIds: putwallSectionIds.join(','),
        },
      });
      setBookStocks(response.data.items);
      setTotalPages(response.data.totalPages);
    } catch (error) {
      console.error('Failed to fetch book stocks:', error);
    } finally {
      setLoading(false);
    }
  };

  const fetchBooksAndPutwalls = async () => {
    try {
        const [booksRes, putwallSectionsRes, putwallsRes] = await Promise.all([
            axios.get('/api/admin/books', { params: { pageSize: 1000 } }),
            axios.get('/api/admin/putwall-sections', { params: { pageSize: 1000 } }),
            axios.get('/api/admin/putwalls', { params: { pageSize: 1000 } })
        ]);
        setAllBooks(booksRes.data.items);
        setAllPutwallSections(putwallSectionsRes.data.items);
        setAllPutwalls(putwallsRes.data.items);
    } catch (error) {
        console.error('Failed to fetch books or putwalls:', error);
    }
  };

  useEffect(() => {
    fetchBookStocks();
  }, [page, query, bookIds, putwallSectionIds]);

  useEffect(() => {
    fetchBooksAndPutwalls();
  }, []);

  useEffect(() => {
    if (selectedPutwall) {
        setFilteredPutwallSections(allPutwallSections.filter(section => section.putwall.id === selectedPutwall.id));
        setSelectedPutwallSection(null); // Reset section selection when putwall changes
    } else {
        setFilteredPutwallSections([]);
    }
  }, [selectedPutwall, allPutwallSections]);

  const openModalForCreate = () => {
    setEditingBookStock(null);
    setSelectedBook(null);
    setSelectedPutwall(null);
    setSelectedPutwallSection(null);
    setIsModalOpen(true);
  };

  const openModalForEdit = (stock: BookStock) => {
    const section = allPutwallSections.find(s => s.row === stock.row && s.column === stock.column && s.putwall.name === stock.putwall);
    const putwall = allPutwalls.find(p => p.id === section?.putwall.id) || null;
    
    setEditingBookStock(stock);
    setSelectedBook(allBooks.find(b => b.id === stock.book.id) || null);
    setSelectedPutwall(putwall);
    // This will trigger the useEffect to filter sections
    setSelectedPutwallSection(section || null);
    setIsModalOpen(true);
  };

  const handleFormSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedBook || !selectedPutwallSection) return;
    
    const payload = {
        bookId: selectedBook.id,
        putwallSectionId: selectedPutwallSection.id,
    };

    const url = editingBookStock ? `/api/admin/book-stocks/${editingBookStock.id}` : '/api/admin/book-stocks';
    const method = editingBookStock ? 'put' : 'post';

    try {
      await axios[method](url, payload);
      setIsModalOpen(false);
      fetchBookStocks();
    } catch (error) {
      console.error(`Failed to ${editingBookStock ? 'update' : 'create'} book stock:`, error);
    }
  };

  const handleDeleteBookStock = async (id: number) => {
    if (window.confirm('Are you sure you want to delete this book stock?')) {
      try {
        await axios.delete(`/api/admin/book-stocks/${id}`);
        fetchBookStocks();
      } catch (error) {
        console.error('Failed to delete book stock:', error);
      }
    }
  };
   

  const distinctPutwalls = Array.from(new Set(allPutwallSections.map(bs => bs.putwall)));
  return (
    <div className="bg-white p-6 rounded-lg shadow-md">
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-2xl font-bold text-gray-800">Book Stocks</h1>
        <button
          onClick={openModalForCreate}
          className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 transition-colors"
        >
          Create Book Stock
        </button>
      </div>
      <div className="mb-4">
        <input
          type="text"
          value={query}
          onChange={(e) => setQuery(e.target.value)}
          placeholder="Search by book name..."
          className="w-full px-4 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
        />
      </div>
      {loading ? (
        <div className="text-center py-10"><p className="text-gray-500">Loading...</p></div>
      ) : (
        <div className="overflow-x-auto">
          <table className="min-w-full bg-white">
            <thead className="bg-gray-50">
              <tr>
                <th className="py-3 px-4 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Id</th>
                <th className="py-3 px-4 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Book Name</th>
                <th className="py-3 px-4 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Putwall</th>
                <th className="py-3 px-4 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Location</th>
                <th className="py-3 px-4 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-200">
              {bookStocks.map((bs) => (
                <tr key={bs.id} className="hover:bg-gray-50">
                  <td className="py-4 px-4 whitespace-nowrap">{bs.id}</td>
                  <td className="py-4 px-4 whitespace-nowrap">{bs.book.name}</td>
                  <td className="py-4 px-4 whitespace-nowrap">{bs.putwall}</td>
                  <td className="py-4 px-4 whitespace-nowrap">{`Row: ${bs.row}, Col: ${bs.column}`}</td>
                  <td className="py-4 px-4 whitespace-nowrap">
                    <div className="flex space-x-2">
                      <button onClick={() => openModalForEdit(bs)} className="text-blue-600 hover:text-blue-800">
                        <Pencil size={20} />
                      </button>
                      <button onClick={() => handleDeleteBookStock(bs.id)} className="text-red-600 hover:text-red-800">
                        <Trash2 size={20} />
                      </button>
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

      <Modal isOpen={isModalOpen} onClose={() => setIsModalOpen(false)} title={editingBookStock ? 'Edit Book Stock' : 'Create Book Stock'}>
        <form onSubmit={handleFormSubmit}>
          <div className="space-y-4">
            <div>
              <label htmlFor="book" className="block text-sm font-medium text-gray-700">Book</label>
              <SingleCustomDropdown
                options={allBooks}
                selected={selectedBook}
                onChange={setSelectedBook}
                placeholder="Select a book"
              />
            </div>
            <div>
              <label htmlFor="putwall" className="block text-sm font-medium text-gray-700">Putwall</label>
              <SingleCustomDropdown
                options={allPutwalls}
                selected={selectedPutwall}
                onChange={setSelectedPutwall}
                placeholder="Select a putwall"
              />
            </div>
            <div>
              <label htmlFor="putwallSection" className="block text-sm font-medium text-gray-700">Putwall Section</label>
              <SingleCustomDropdown
                options={filteredPutwallSections}
                selected={selectedPutwallSection}
                onChange={setSelectedPutwallSection}
                placeholder="Select a section"
                getOptionLabel={(option) => `${option.putwall.name} - (${option.row}, ${option.column})`}
                disabled={!selectedPutwall}
              />
            </div>
            <div className="flex justify-end space-x-2 pt-4">
              <button type="button" onClick={() => setIsModalOpen(false)} className="px-4 py-2 text-sm font-medium text-gray-700 bg-gray-100 rounded-md hover:bg-gray-200">Cancel</button>
              <button 
                type="submit" 
                className="px-4 py-2 text-sm font-medium text-white bg-blue-600 rounded-md hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed"
                disabled={!selectedBook || !selectedPutwallSection}
              >
                {editingBookStock ? 'Update' : 'Create'}
              </button>
            </div>
          </div>
        </form>
      </Modal>
    </div>
  );
}

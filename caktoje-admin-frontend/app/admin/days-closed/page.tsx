'use client';

import { useEffect, useState } from 'react';
import axios from '../../lib/axios';
import Modal from '../../components/Modal';
import { Pencil, Trash2 } from 'lucide-react';
import SearchableDropdown from '@/app/components/SearchableDropdown';

interface DayClosed {
  id: number;
  date: string;
  recurringType: RecurringType;
}

interface PaginatedDaysClosed {
  items: DayClosed[];
  totalPages: number;
}
type RecurringType = 'weekly' | 'monthly' | 'biweekly' | 'once' | 'yearly';

export default function DaysClosedPage() {
  const [daysClosed, setDaysClosed] = useState<DayClosed[]>([]);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [loading, setLoading] = useState(true);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingDay, setEditingDay] = useState<DayClosed | null>(null);

  const [date, setDate] = useState('');
  const [recurringType, setRecurringType] = useState<RecurringType>('once');

  const fetchDaysClosed = async () => {
    setLoading(true);
    try {
      const response = await axios.get<PaginatedDaysClosed>('/api/admin/days-closed', {
        params: { page, pageSize: 10 },
      });
      setDaysClosed(response.data.items);
      setTotalPages(response.data.totalPages);
    } catch (error) {
      console.error('Failed to fetch days closed:', error);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchDaysClosed();
  }, [page]);

  const openModalForCreate = () => {
    setEditingDay(null);
    setDate('');
    setIsModalOpen(true);
    setRecurringType('once');
  };

  const openModalForEdit = (day: DayClosed) => {
    setEditingDay(day);
    setDate(new Date(day.date).toISOString().split('T')[0]);
    setRecurringType(day.recurringType);
    setIsModalOpen(true);
  };

  const handleFormSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    const payload = { date, recurringType };
    const url = editingDay ? `/api/admin/days-closed/${editingDay.id}` : '/api/admin/days-closed';
    const method = editingDay ? 'put' : 'post';

    try {
      await axios[method](url, payload);
      setIsModalOpen(false);
      fetchDaysClosed();
    } catch (error) {
      console.error(`Failed to ${editingDay ? 'update' : 'create'} day closed:`, error);
    }
  };

  const handleDeleteDay = async (id: number) => {
    if (window.confirm('Are you sure you want to delete this closed day?')) {
      try {
        await axios.delete(`/api/admin/days-closed/${id}`);
        fetchDaysClosed();
      } catch (error) {
        console.error('Failed to delete day closed:', error);
      }
    }
  };

  return (
    <div className="bg-white p-6 rounded-lg shadow-md">
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-2xl font-bold text-gray-800">Days Closed</h1>
        <button
          onClick={openModalForCreate}
          className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 transition-colors"
        >
          Add Day Closed
        </button>
      </div>
      {loading ? (
        <div className="text-center py-10"><p className="text-gray-500">Loading...</p></div>
      ) : (
        <div className="overflow-x-auto">
          <table className="min-w-full bg-white">
            <thead className="bg-gray-50">
              <tr>
                <th className="py-3 px-4 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Date</th>
                <th className="py-3 px-4 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Recurring Type</th>
                <th className="py-3 px-4 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-200">
              {daysClosed.map((day) => (
                <tr key={day.id} className="hover:bg-gray-50">
                  <td className="py-4 px-4 whitespace-nowrap">{new Date(day.date).toLocaleDateString()}</td>
                  <td className="py-4 px-4 whitespace-nowrap">{day.recurringType}</td>
                  <td className="py-4 px-4 whitespace-nowrap">
                    <div className="flex space-x-2">
                      <button onClick={() => openModalForEdit(day)} className="text-blue-600 hover:text-blue-800">
                        <Pencil size={20} />
                      </button>
                      <button onClick={() => handleDeleteDay(day.id)} className="text-red-600 hover:text-red-800">
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

      <Modal isOpen={isModalOpen} onClose={() => setIsModalOpen(false)} title={editingDay ? 'Edit Day Closed' : 'Add Day Closed'}>
        <form onSubmit={handleFormSubmit}>
          <div className="space-y-4">
            <div>
              <label htmlFor="date" className="block text-sm font-medium text-gray-700">Date</label>
              <input type="date" id="date" value={date} onChange={(e) => setDate(e.target.value)} className="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500" required />
            </div>
            <div>
                <SearchableDropdown
                                    options={[{id: 'once', name: 'Once'}, {id: 'weekly', name: 'Weekly'}, {id: 'biweekly', name: 'Biweekly'}, {id: 'monthly', name: 'Monthly'}, {id: 'yearly', name: 'Yearly'}]}
                                    selected={recurringType ? {id: recurringType, name: recurringType.charAt(0).toUpperCase() + recurringType.slice(1)} : null}
                                    onChange={(val) => setRecurringType(val ? val.id as RecurringType : 'once')}
                                    placeholder="Select Recurring Type..."
                                    getOptionLabel={(option) => option.name}
                                />
            </div>
            <div className="flex justify-end space-x-2 pt-4">
              <button type="button" onClick={() => setIsModalOpen(false)} className="px-4 py-2 text-sm font-medium text-gray-700 bg-gray-100 rounded-md hover:bg-gray-200">Cancel</button>
              <button type="submit" className="px-4 py-2 text-sm font-medium text-white bg-blue-600 rounded-md hover:bg-blue-700">{editingDay ? 'Update' : 'Add'}</button>
            </div>
          </div>
        </form>
      </Modal>
    </div>
  );
}

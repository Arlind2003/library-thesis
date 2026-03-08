'use client';

import { useEffect, useState } from 'react';
import axios from '../../lib/axios';
import Modal from '../../components/Modal';
import { Pencil, Trash2 } from 'lucide-react';

interface Putwall {
  id: number;
  rows: number;
  columns: number;
  name: string;
}

interface PaginatedPutwalls {
  items: Putwall[];
  totalPages: number;
}

export default function PutwallsPage() {
  const [putwalls, setPutwalls] = useState<Putwall[]>([]);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [query, setQuery] = useState('');
  const [loading, setLoading] = useState(true);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingPutwall, setEditingPutwall] = useState<Putwall | null>(null);

  // Form state
  const [putwallName, setPutwallName] = useState('');
  const [rows, setRows] = useState(0);
  const [columns, setColumns] = useState(0);

  const fetchPutwalls = async () => {
    setLoading(true);
    try {
      const response = await axios.get<PaginatedPutwalls>('/api/admin/putwalls', {
        params: { query, page, pageSize: 10 },
      });
      setPutwalls(response.data.items);
      setTotalPages(response.data.totalPages);
    } catch (error) {
      console.error('Failed to fetch putwalls:', error);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchPutwalls();
  }, [page, query]);

  const openModalForCreate = () => {
    setEditingPutwall(null);
    setPutwallName('');
    setIsModalOpen(true);
  };

  const openModalForEdit = (putwall: Putwall) => {
    setEditingPutwall(putwall);
    setPutwallName(putwall.name);
    setRows(putwall.rows);
    setColumns(putwall.columns);
    setIsModalOpen(true);
  };

  const handleFormSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    const payload = { name: putwallName, rows, columns };
    const url = editingPutwall ? `/api/admin/putwalls/${editingPutwall.id}` : '/api/admin/putwalls';
    const method = editingPutwall ? 'put' : 'post';

    try {
      await axios[method](url, payload);
      setIsModalOpen(false);
      fetchPutwalls();
    } catch (error) {
      console.error(`Failed to ${editingPutwall ? 'update' : 'create'} putwall:`, error);
    }
  };

  const handleDeletePutwall = async (id: number) => {
    if (window.confirm('Are you sure you want to delete this putwall?')) {
      try {
        await axios.delete(`/api/admin/putwalls/${id}`);
        fetchPutwalls();
      } catch (error) {
        console.error('Failed to delete putwall:', error);
      }
    }
  };

  return (
    <div className="bg-white p-6 rounded-lg shadow-md">
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-2xl font-bold text-gray-800">Putwalls</h1>
        <button
          onClick={openModalForCreate}
          className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 transition-colors"
        >
          Create Putwall
        </button>
      </div>
      <div className="mb-4">
        <input
          type="text"
          value={query}
          onChange={(e) => setQuery(e.target.value)}
          placeholder="Search putwalls..."
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
                <th className="py-3 px-4 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Name</th>
                <th className="py-3 px-4 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Rows</th>
                <th className="py-3 px-4 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Columns</th>
                <th className="py-3 px-4 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-200">
              {putwalls.map((putwall) => (
                <tr key={putwall.id} className="hover:bg-gray-50">
                  <td className="py-4 px-4 whitespace-nowrap">{putwall.name}</td>
                  <td className="py-4 px-4 whitespace-nowrap">{putwall.rows}</td>
                  <td className="py-4 px-4 whitespace-nowrap">{putwall.columns}</td>
                  <td className="py-4 px-4 whitespace-nowrap">
                    <div className="flex space-x-2">
                      <button onClick={() => openModalForEdit(putwall)} className="text-blue-600 hover:text-blue-800">
                        <Pencil size={20} />
                      </button>
                      <button onClick={() => handleDeletePutwall(putwall.id)} className="text-red-600 hover:text-red-800">
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

      <Modal isOpen={isModalOpen} onClose={() => setIsModalOpen(false)} title={editingPutwall ? 'Edit Putwall' : 'Create Putwall'}>
        <form onSubmit={handleFormSubmit}>
          <div className="space-y-4">
            <div>
              <label htmlFor="name" className="block text-sm font-medium text-gray-700">Name</label>
              <input type="text" id="name" value={putwallName} onChange={(e) => setPutwallName(e.target.value)} className="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500" required />
            </div>
            <div>
              <label htmlFor="rows" className="block text-sm font-medium text-gray-700">Rows</label>
              <input type="number" id="rows" value={rows} onChange={(e) => setRows(Number(e.target.value))} className="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500" required />
            </div>
            <div>
              <label htmlFor="columns" className="block text-sm font-medium text-gray-700">Columns</label>
              <input type="number" id="columns" value={columns} onChange={(e) => setColumns(Number(e.target.value))} className="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500" required />
            </div>
            <div className="flex justify-end space-x-2 pt-4">
              <button type="button" onClick={() => setIsModalOpen(false)} className="px-4 py-2 text-sm font-medium text-gray-700 bg-gray-100 rounded-md hover:bg-gray-200">Cancel</button>
              <button type="submit" className="px-4 py-2 text-sm font-medium text-white bg-blue-600 rounded-md hover:bg-blue-700">{editingPutwall ? 'Update' : 'Create'}</button>
            </div>
          </div>
        </form>
      </Modal>
    </div>
  );
}

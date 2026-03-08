'use client';

import { useEffect, useState } from 'react';
import axios from '../../lib/axios';
import Modal from '../../components/Modal';
import { Pencil, Trash2 } from 'lucide-react';
import SingleCustomDropdown from '../../components/SingleCustomDropdown';

interface PutwallSection {
  id: number;
  putwall: {
    id: number;
    name: string;
  };
  row: number;
  column: number;
}

interface PaginatedPutwallSections {
  items: PutwallSection[];
  totalPages: number;
}

interface Putwall {
    id: number;
    name: string;
    rows: number;
    columns: number;
}

export default function PutwallSectionsPage() {
  const [putwallSections, setPutwallSections] = useState<PutwallSection[]>([]);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [query, setQuery] = useState('');
  const [putwallIds, setPutwallIds] = useState<number[]>([]);
  const [loading, setLoading] = useState(true);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingSection, setEditingSection] = useState<PutwallSection | null>(null);

  // Form state
  const [selectedPutwall, setSelectedPutwall] = useState<Putwall | null>(null);
  const [row, setRow] = useState(1);
  const [column, setColumn] = useState(1);
  const [errors, setErrors] = useState<{ row?: string; column?: string }>({});

  const [allPutwalls, setAllPutwalls] = useState<Putwall[]>([]);

  const fetchPutwallSections = async () => {
    setLoading(true);
    try {
      const response = await axios.get<PaginatedPutwallSections>('/api/admin/putwall-sections', {
        params: {
          query,
          page,
          pageSize: 10,
          putwallIds: putwallIds.join(','),
        },
      });
      setPutwallSections(response.data.items);
      setTotalPages(response.data.totalPages);
    } catch (error) {
      console.error('Failed to fetch putwall sections:', error);
    } finally {
      setLoading(false);
    }
  };

  const fetchPutwalls = async () => {
      try {
          const response = await axios.get('/api/admin/putwalls', { params: { pageSize: 1000 } });
          setAllPutwalls(response.data.items);
      } catch (error) {
          console.error('Failed to fetch putwalls:', error);
      }
  };

  useEffect(() => {
    fetchPutwallSections();
  }, [page, query, putwallIds]);

  useEffect(() => {
    fetchPutwalls();
  }, []);

  useEffect(() => {
    const newErrors: { row?: string; column?: string } = {};
    if (selectedPutwall) {
      if (row < 1 || row > selectedPutwall.rows) {
        newErrors.row = `Row must be between 1 and ${selectedPutwall.rows}`;
      }
      if (column < 1 || column > selectedPutwall.columns) {
        newErrors.column = `Column must be between 1 and ${selectedPutwall.columns}`;
      }
    }
    setErrors(newErrors);
  }, [row, column, selectedPutwall]);


  const openModalForCreate = () => {
    setEditingSection(null);
    setSelectedPutwall(null);
    setRow(1);
    setColumn(1);
    setErrors({});
    setIsModalOpen(true);
  };

  const openModalForEdit = (section: PutwallSection) => {
    const putwall = allPutwalls.find(p => p.id === section.putwall.id) || null;
    setEditingSection(section);
    setSelectedPutwall(putwall);
    setRow(section.row);
    setColumn(section.column);
    setErrors({});
    setIsModalOpen(true);
  };

  const handleFormSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (Object.keys(errors).length > 0 || !selectedPutwall) return;

    const payload = {
        putwallId: selectedPutwall.id,
        row,
        column
    };
    const url = editingSection ? `/api/admin/putwall-sections/${editingSection.id}` : '/api/admin/putwall-sections';
    const method = editingSection ? 'put' : 'post';

    try {
      await axios[method](url, payload);
      setIsModalOpen(false);
      fetchPutwallSections();
    } catch (error) {
      console.error(`Failed to ${editingSection ? 'update' : 'create'} putwall section:`, error);
    }
  };

  const handleDeleteSection = async (id: number) => {
    if (window.confirm('Are you sure you want to delete this putwall section?')) {
      try {
        await axios.delete(`/api/admin/putwall-sections/${id}`);
        fetchPutwallSections();
      } catch (error) {
        console.error('Failed to delete putwall section:', error);
      }
    }
  };

  return (
    <div className="bg-white p-6 rounded-lg shadow-md">
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-2xl font-bold text-gray-800">Putwall Sections</h1>
        <button
          onClick={openModalForCreate}
          className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 transition-colors"
        >
          Create Section
        </button>
      </div>
      <div className="mb-4">
        <input
          type="text"
          value={query}
          onChange={(e) => setQuery(e.target.value)}
          placeholder="Search by putwall name..."
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
                <th className="py-3 px-4 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Putwall</th>
                <th className="py-3 px-4 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Row Location</th>
                <th className="py-3 px-4 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Column Location</th>
                <th className="py-3 px-4 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-200">
              {putwallSections.map((section) => (
                <tr key={section.id} className="hover:bg-gray-50">
                  <td className="py-4 px-4 whitespace-nowrap">{section.putwall.name}</td>
                  <td className="py-4 px-4 whitespace-nowrap">{section.row}</td>
                  <td className="py-4 px-4 whitespace-nowrap">{section.column}</td>
                  <td className="py-4 px-4 whitespace-nowrap">
                    <div className="flex space-x-2">
                      <button onClick={() => openModalForEdit(section)} className="text-blue-600 hover:text-blue-800">
                        <Pencil size={20} />
                      </button>
                      <button onClick={() => handleDeleteSection(section.id)} className="text-red-600 hover:text-red-800">
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

      <Modal isOpen={isModalOpen} onClose={() => setIsModalOpen(false)} title={editingSection ? 'Edit Section' : 'Create Section'}>
        <form onSubmit={handleFormSubmit}>
          <div className="space-y-4">
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
              <label htmlFor="row" className="block text-sm font-medium text-gray-700">Row</label>
              <input
                type="number"
                id="row"
                value={row}
                onChange={(e) => setRow(Number(e.target.value))}
                className={`mt-1 block w-full px-3 py-2 border rounded-md shadow-sm ${!selectedPutwall ? 'bg-gray-100' : 'border-gray-300 focus:ring-blue-500 focus:border-blue-500'}`}
                required
                disabled={!selectedPutwall}
              />
              {errors.row && <p className="mt-1 text-sm text-red-600">{errors.row}</p>}
            </div>
            <div>
              <label htmlFor="column" className="block text-sm font-medium text-gray-700">Column</label>
              <input
                type="number"
                id="column"
                value={column}
                onChange={(e) => setColumn(Number(e.target.value))}
                className={`mt-1 block w-full px-3 py-2 border rounded-md shadow-sm ${!selectedPutwall ? 'bg-gray-100' : 'border-gray-300 focus:ring-blue-500 focus:border-blue-500'}`}
                required
                disabled={!selectedPutwall}
              />
              {errors.column && <p className="mt-1 text-sm text-red-600">{errors.column}</p>}
            </div>
            <div className="flex justify-end space-x-2 pt-4">
              <button type="button" onClick={() => setIsModalOpen(false)} className="px-4 py-2 text-sm font-medium text-gray-700 bg-gray-100 rounded-md hover:bg-gray-200">Cancel</button>
              <button
                type="submit"
                className="px-4 py-2 text-sm font-medium text-white bg-blue-600 rounded-md hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed"
                disabled={Object.keys(errors).length > 0 || !selectedPutwall || row === 0 || column === 0}
              >
                {editingSection ? 'Update' : 'Create'}
              </button>
            </div>
          </div>
        </form>
      </Modal>
    </div>
  );
}

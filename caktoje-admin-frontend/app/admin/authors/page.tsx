'use client';

import { useEffect, useState } from 'react';
import axios from '../../lib/axios';
import Modal from '../../components/Modal';
import { Pencil, Trash2 } from 'lucide-react';

// Define interfaces for your data
interface Author {
  id: number;
  name: string;
  biography: string;
  image: { fileName: string };
}

interface PaginatedAuthors {
  items: Author[];
  totalPages: number;
}

export default function AuthorsPage() {
  const [authors, setAuthors] = useState<Author[]>([]);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [query, setQuery] = useState('');
  const [loading, setLoading] = useState(true);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [newAuthorName, setNewAuthorName] = useState('');
  const [newAuthorBiography, setNewAuthorBiography] = useState('');
  const [newAuthorImage, setNewAuthorImage] = useState<File | null>(null);
  const [editingAuthor, setEditingAuthor] = useState<Author | null>(null);

  const fetchAuthors = async () => {
    setLoading(true);
    try {
      const response = await axios.get<PaginatedAuthors>('/api/admin/authors', {
        params: { query, page, pageSize: 10 },
      });
      setAuthors(response.data.items);
      setTotalPages(response.data.totalPages);
    } catch (error) {
      console.error('Failed to fetch authors:', error);
    } finally {
      setLoading(false);
    }
  };

  const openModalForCreate = () => {
    setEditingAuthor(null);
    setNewAuthorName('');
    setNewAuthorBiography('');
    setNewAuthorImage(null);
    setIsModalOpen(true);
  };

  const openModalForEdit = (author: Author) => {
    setEditingAuthor(author);
    setNewAuthorName(author.name);
    setNewAuthorBiography(author.biography);
    setNewAuthorImage(null);
    setIsModalOpen(true);
  };

  const handleFormSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    const formData = new FormData();
    formData.append('fullName', newAuthorName);
    formData.append('biography', newAuthorBiography);
    if (newAuthorImage) {
      formData.append('image', newAuthorImage);
    }

    const url = editingAuthor ? `/api/admin/authors/${editingAuthor.id}` : '/api/admin/authors';
    const method = editingAuthor ? 'put' : 'post';

    try {
      await axios[method](url, formData, {
        headers: {
          'Content-Type': 'multipart/form-data',
        },
      });
      setIsModalOpen(false);
      fetchAuthors(); // Refresh the list
    } catch (error) {
      console.error(`Failed to ${editingAuthor ? 'update' : 'create'} author:`, error);
    }
  };

  const handleDeleteAuthor = async (id: number) => {
    if (window.confirm('Are you sure you want to delete this author?')) {
      try {
        await axios.delete(`/api/admin/authors/${id}`);
        fetchAuthors(); // Refresh the list
      } catch (error) {
        console.error('Failed to delete author:', error);
      }
    }
  };

  useEffect(() => {
    fetchAuthors();
  }, [page, query]);

  return (
    <div className="bg-white p-6 rounded-lg shadow-md">
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-2xl font-bold text-gray-800">Authors</h1>
        <button
          onClick={openModalForCreate}
          className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 transition-colors"
        >
          Create Author
        </button>
      </div>
      <div className="mb-4">
        <input
          type="text"
          value={query}
          onChange={(e) => setQuery(e.target.value)}
          placeholder="Search authors..."
          className="w-full px-4 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
        />
      </div>
      {loading ? (
        <div className="text-center py-10">
          <p className="text-gray-500">Loading...</p>
        </div>
      ) : (
        <div className="overflow-x-auto">
          <table className="min-w-full bg-white">
            <thead className="bg-gray-50">
              <tr>
                <th className="py-3 px-4 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Name</th>
                <th className="py-3 px-4 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Biography</th>
                <th className="py-3 px-4 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-200">
              {authors.map((author) => (
                <tr key={author.id} className="hover:bg-gray-50">
                  <td className="py-4 px-4 whitespace-nowrap">{author.name}</td>
                  <td className="py-4 px-4 whitespace-nowrap">{author.biography}</td>
                  <td className="py-4 px-4 whitespace-nowrap">
                    <div className="flex space-x-2">
                      <button onClick={() => openModalForEdit(author)} className="text-blue-600 hover:text-blue-800">
                        <Pencil size={20} />
                      </button>
                      <button onClick={() => handleDeleteAuthor(author.id)} className="text-red-600 hover:text-red-800">
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
        <span className="text-sm text-gray-700">
          Page {page} of {totalPages}
        </span>
        <button
          onClick={() => setPage(page + 1)}
          disabled={page === totalPages}
          className="px-4 py-2 bg-gray-200 text-gray-800 rounded-md disabled:opacity-50 hover:bg-gray-300"
        >
          Next
        </button>
      </div>

      <Modal isOpen={isModalOpen} onClose={() => setIsModalOpen(false)} title={editingAuthor ? 'Edit Author' : 'Create Author'}>
        <form onSubmit={handleFormSubmit}>
          <div className="space-y-4">
            <div>
              <label htmlFor="name" className="block text-sm font-medium text-gray-700">
                Full Name
              </label>
              <input
                type="text"
                id="name"
                value={newAuthorName}
                onChange={(e) => setNewAuthorName(e.target.value)}
                className="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500"
                required
              />
            </div>
            <div>
              <label htmlFor="biography" className="block text-sm font-medium text-gray-700">
                Biography
              </label>
              <textarea
                id="biography"
                value={newAuthorBiography}
                onChange={(e) => setNewAuthorBiography(e.target.value)}
                className="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500"
              />
            </div>
            <div>
              <label htmlFor="image" className="block text-sm font-medium text-gray-700">
                Image
              </label>
              <input
                type="file"
                id="image"
                onChange={(e) => setNewAuthorImage(e.target.files ? e.target.files[0] : null)}
                className="mt-1 block w-full text-sm text-gray-500 file:mr-4 file:py-2 file:px-4 file:rounded-full file:border-0 file:text-sm file:font-semibold file:bg-blue-50 file:text-blue-700 hover:file:bg-blue-100"
              />
            </div>
            <div className="flex justify-end space-x-2 pt-4">
              <button
                type="button"
                onClick={() => setIsModalOpen(false)}
                className="px-4 py-2 text-sm font-medium text-gray-700 bg-gray-100 rounded-md hover:bg-gray-200"
              >
                Cancel
              </button>
              <button
                type="submit"
                className="px-4 py-2 text-sm font-medium text-white bg-blue-600 rounded-md hover:bg-blue-700"
              >
                {editingAuthor ? 'Update' : 'Create'}
              </button>
            </div>
          </div>
        </form>
      </Modal>
    </div>
  );
}

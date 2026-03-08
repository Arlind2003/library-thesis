'use client';

import { useEffect, useState } from 'react';
import axios from '../../lib/axios';
import Modal from '../../components/Modal';
import { Pencil, Trash2 } from 'lucide-react';
import CustomDropdown from '../../components/CustomDropdown';

interface Book {
  id: number;
  name: string;
  description: string;
  image: { fileName: string };
  authors: { id: number; name: string }[];
  categories: { id: number; name: string }[];
}

interface PaginatedBooks {
  items: Book[];
  totalPages: number;
}

interface Author {
  id: number;
  name: string;
}

interface Category {
  id: number;
  name: string;
}

export default function BooksPage() {
  const [books, setBooks] = useState<Book[]>([]);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [query, setQuery] = useState('');
  const [authorIds, setAuthorIds] = useState<number[]>([]);
  const [categoryIds, setCategoryIds] = useState<number[]>([]);
  const [loading, setLoading] = useState(true);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingBook, setEditingBook] = useState<Book | null>(null);

  const [bookName, setBookName] = useState('');
  const [isbn, setIsbn] = useState('');
  const [bookDescription, setBookDescription] = useState('');
  const [bookImage, setBookImage] = useState<File | null>(null);
  const [selectedAuthors, setSelectedAuthors] = useState<{id: number, name: string}[]>([]);
  const [selectedCategories, setSelectedCategories] = useState<{id: number, name: string}[]>([]);

  const [allAuthors, setAllAuthors] = useState<Author[]>([]);
  const [allCategories, setAllCategories] = useState<Category[]>([]);

  const fetchBooks = async () => {
    setLoading(true);
    try {
      const response = await axios.get<PaginatedBooks>('/api/admin/books', {
        params: {
          query,
          page,
          pageSize: 10,
          authorIds: authorIds.join(','),
          categoryIds: categoryIds.join(','),
        },
      });
      setBooks(response.data.items);
      setTotalPages(response.data.totalPages);
    } catch (error) {
      console.error('Failed to fetch books:', error);
    } finally {
      setLoading(false);
    }
  };

  const fetchAuthorsAndCategories = async () => {
    try {
      const [authorsRes, categoriesRes] = await Promise.all([
        axios.get('/api/admin/authors', { params: { pageSize: 1000 } }),
        axios.get('/api/admin/categories', { params: { pageSize: 1000 } })
      ]);
      setAllAuthors(authorsRes.data.items);
      setAllCategories(categoriesRes.data.items);
    } catch (error) {
      console.error('Failed to fetch authors or categories:', error);
    }
  };

  useEffect(() => {
    fetchBooks();
  }, [page, query, authorIds, categoryIds]);

  useEffect(() => {
    fetchAuthorsAndCategories();
  }, []);

  const openModalForCreate = () => {
    setEditingBook(null);
    setBookName('');
    setBookDescription('');
    setBookImage(null);
    setSelectedAuthors([]);
    setSelectedCategories([]);
    setIsModalOpen(true);
  };

  const openModalForEdit = (book: Book) => {
    setEditingBook(book);
    setBookName(book.name);
    setBookDescription(book.description);
    setBookImage(null);
    setSelectedAuthors(book.authors);
    setSelectedCategories(book.categories);
    setIsModalOpen(true);
  };

  const handleFormSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    const formData = new FormData();
    formData.append('name', bookName);
    formData.append('description', bookDescription);
    formData.append('isbn', isbn);
    if (bookImage) {
      formData.append('image', bookImage);
    }
    selectedAuthors.forEach(author => formData.append('authorIds', String(author.id)));
    selectedCategories.forEach(category => formData.append('categoryIds', String(category.id)));

    const url = editingBook ? `/api/admin/books/${editingBook.id}` : '/api/admin/books';
    const method = editingBook ? 'put' : 'post';

    try {
      await axios[method](url, formData, {
        headers: { 'Content-Type': 'multipart/form-data' },
      });
      setIsModalOpen(false);
      fetchBooks();
    } catch (error) {
      console.error(`Failed to ${editingBook ? 'update' : 'create'} book:`, error);
    }
  };

  const handleDeleteBook = async (id: number) => {
    if (window.confirm('Are you sure you want to delete this book?')) {
      try {
        await axios.delete(`/api/admin/books/${id}`);
        fetchBooks();
      } catch (error) {
        console.error('Failed to delete book:', error);
      }
    }
  };

  return (
    <div className="bg-white p-6 rounded-lg shadow-md">
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-2xl font-bold text-gray-800">Books</h1>
        <button
          onClick={openModalForCreate}
          className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 transition-colors"
        >
          Create Book
        </button>
      </div>
      <div className="mb-4">
        <input
          type="text"
          value={query}
          onChange={(e) => setQuery(e.target.value)}
          placeholder="Search books..."
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
                <th className="py-3 px-4 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Authors</th>
                <th className="py-3 px-4 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Categories</th>
                <th className="py-3 px-4 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-200">
              {books.map((book) => (
                <tr key={book.id} className="hover:bg-gray-50">
                  <td className="py-4 px-4 whitespace-nowrap">{book.name}</td>
                  <td className="py-4 px-4 whitespace-nowrap">{book.authors.map(a => a.name).join(', ')}</td>
                  <td className="py-4 px-4 whitespace-nowrap">{book.categories.map(c => c.name).join(', ')}</td>
                  <td className="py-4 px-4 whitespace-nowrap">
                    <div className="flex space-x-2">
                      <button onClick={() => openModalForEdit(book)} className="text-blue-600 hover:text-blue-800">
                        <Pencil size={20} />
                      </button>
                      <button onClick={() => handleDeleteBook(book.id)} className="text-red-600 hover:text-red-800">
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

      <Modal isOpen={isModalOpen} onClose={() => setIsModalOpen(false)} title={editingBook ? 'Edit Book' : 'Create Book'}>
        <form onSubmit={handleFormSubmit}>
          <div className="space-y-4">
            <div>
              <label htmlFor="name" className="block text-sm font-medium text-gray-700">Name</label>
              <input type="text" id="name" value={bookName} onChange={(e) => setBookName(e.target.value)} className="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500" required />
            </div>
            <div>
              <label htmlFor="isbn" className="block text-sm font-medium text-gray-700">ISBN</label>
              <input type="text" id="isbn" value={isbn} onChange={(e) => setIsbn(e.target.value)} className="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500" required />
            </div>
            <div>
              <label htmlFor="description" className="block text-sm font-medium text-gray-700">Description</label>
              <textarea id="description" value={bookDescription} onChange={(e) => setBookDescription(e.target.value)} className="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500" />
            </div>
            <div>
              <label htmlFor="authors" className="block text-sm font-medium text-gray-700">Authors</label>
              <CustomDropdown
                options={allAuthors}
                selected={selectedAuthors}
                onChange={setSelectedAuthors}
                placeholder="Select authors..."
              />
            </div>
            <div>
              <label htmlFor="categories" className="block text-sm font-medium text-gray-700">Categories</label>
              <CustomDropdown
                options={allCategories}
                selected={selectedCategories}
                onChange={setSelectedCategories}
                placeholder="Select categories..."
              />
            </div>
            <div>
              <label htmlFor="image" className="block text-sm font-medium text-gray-700">Image</label>
              <input type="file" id="image" onChange={(e) => setBookImage(e.target.files ? e.target.files[0] : null)} className="mt-1 block w-full text-sm text-gray-500 file:mr-4 file:py-2 file:px-4 file:rounded-full file:border-0 file:text-sm file:font-semibold file:bg-blue-50 file:text-blue-700 hover:file:bg-blue-100" />
            </div>
            <div className="flex justify-end space-x-2 pt-4">
              <button type="button" onClick={() => setIsModalOpen(false)} className="px-4 py-2 text-sm font-medium text-gray-700 bg-gray-100 rounded-md hover:bg-gray-200">Cancel</button>
              <button type="submit" className="px-4 py-2 text-sm font-medium text-white bg-blue-600 rounded-md hover:bg-blue-700">{editingBook ? 'Update' : 'Create'}</button>
            </div>
          </div>
        </form>
      </Modal>
    </div>
  );
}

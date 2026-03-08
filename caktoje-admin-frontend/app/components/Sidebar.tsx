'use client';

import Link from 'next/link';
import { usePathname } from 'next/navigation';
import { Book, Shapes, Users, Box, Calendar, Key, LogOut } from 'lucide-react';
import { cn } from '../lib/utils';

const navigation = [
  { name: 'Authors', href: '/admin/authors', icon: Users },
  { name: 'Books', href: '/admin/books', icon: Book },
  { name: 'Book Stocks', href: '/admin/book-stocks', icon: Box },
  { name: 'Book Rents', href: '/admin/book-rents', icon: Calendar },
  { name: 'Categories', href: '/admin/categories', icon: Shapes },
  //{ name: 'Days Closed', href: '/admin/days-closed', icon: Calendar },
  { name: 'Putwalls', href: '/admin/putwalls', icon: Key },
  { name: 'Putwall Sections', href: '/admin/putwall-sections', icon: Key },
  { name: 'Users', href: '/admin/users', icon: Users },
];

export default function Sidebar() {
  const pathname = usePathname();

  return (
    <div className="flex flex-col w-64 bg-white shadow-lg">
      <div className="flex items-center justify-center h-20 border-b">
        <h1 className="text-2xl font-bold text-blue-600">Caktoje Admin</h1>
      </div>
      <nav className="flex-1 px-4 py-4 space-y-2">
        {navigation.map((item) => (
          <Link
            key={item.name}
            href={item.href}
            className={cn(
              'flex items-center px-3 py-2 text-sm font-medium rounded-lg transition-colors',
              pathname.startsWith(item.href)
                ? 'bg-blue-500 text-white shadow-md'
                : 'text-gray-600 hover:bg-gray-100 hover:text-gray-900'
            )}
          >
            <item.icon className="w-5 h-5 mr-3" />
            {item.name}
          </Link>
        ))}
      </nav>
      <div className="px-4 py-4 border-t">
        <button
          onClick={() => {
            // Handle logout
            console.log('Logout');
          }}
          className="flex items-center w-full px-3 py-2 text-sm font-medium text-gray-600 rounded-lg hover:bg-gray-100 hover:text-gray-900"
        >
          <LogOut className="w-5 h-5 mr-3" />
          Logout
        </button>
      </div>
    </div>
  );
}

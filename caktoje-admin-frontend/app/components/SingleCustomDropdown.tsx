'use client';

import { Fragment } from 'react';
import { Listbox, Transition } from '@headlessui/react';
import { Check, ChevronsUpDown } from 'lucide-react';

export interface DropdownOption {
  id: number | string;
  name?: string; // Make name optional
}

interface SingleCustomDropdownProps<T extends DropdownOption> {
  options: T[];
  selected: T | null;
  onChange: (selected: T | null) => void;
  placeholder: string;
  getOptionLabel?: (option: T) => string;
  disabled?: boolean;
}

export default function SingleCustomDropdown<T extends DropdownOption>({
  options,
  selected,
  onChange,
  placeholder,
  getOptionLabel,
  disabled = false,
}: SingleCustomDropdownProps<T>) {
  const getLabel = (option: T | null): string => {
    if (!option) return placeholder;
    if (getOptionLabel) return getOptionLabel(option);
    return option.name || ''; // Fallback to empty string if name is not present
  };

  return (
    <Listbox value={selected} onChange={onChange} disabled={disabled}>
      <div className="relative mt-1">
        <Listbox.Button className="relative w-full cursor-default rounded-md bg-white py-2 pl-3 pr-10 text-left border border-gray-300 shadow-sm focus:outline-none focus:ring-1 focus:ring-blue-500 focus:border-blue-500 sm:text-sm disabled:bg-gray-100 disabled:cursor-not-allowed">
          <span className={`block truncate ${!selected ? 'text-gray-500' : 'text-gray-900'}`}>
            {getLabel(selected)}
          </span>
          <span className="pointer-events-none absolute inset-y-0 right-0 flex items-center pr-2">
            <ChevronsUpDown className="h-5 w-5 text-gray-400" aria-hidden="true" />
          </span>
        </Listbox.Button>
        <Transition
          as={Fragment}
          leave="transition ease-in duration-100"
          leaveFrom="opacity-100"
          leaveTo="opacity-0"
        >
          <Listbox.Options className="absolute z-10 mt-1 max-h-60 w-full overflow-auto rounded-md bg-white py-1 text-base shadow-lg ring-1 ring-black ring-opacity-5 focus:outline-none sm:text-sm">
            {options.map((option) => (
              <Listbox.Option
                key={String(option.id)}
                className={({ active }) =>
                  `relative cursor-default select-none py-2 pl-10 pr-4 ${
                    active ? 'bg-blue-100 text-blue-900' : 'text-gray-900'
                  }`
                }
                value={option}
              >
                {({ selected: isSelected }) => (
                  <>
                    <span
                      className={`block truncate ${
                        isSelected ? 'font-medium' : 'font-normal'
                      }`}
                    >
                      {getLabel(option)}
                    </span>
                    {isSelected ? (
                      <span className="absolute inset-y-0 left-0 flex items-center pl-3 text-blue-600">
                        <Check className="h-5 w-5" aria-hidden="true" />
                      </span>
                    ) : null}
                  </>
                )}
              </Listbox.Option>
            ))}
          </Listbox.Options>
        </Transition>
      </div>
    </Listbox>
  );
}

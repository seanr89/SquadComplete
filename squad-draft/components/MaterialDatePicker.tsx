import React, { useState, useEffect, useRef } from 'react';

interface MaterialDatePickerProps {
  value: string; // YYYY-MM-DD
  onChange: (date: string) => void;
  minDate: Date;
  maxDate: Date;
}

const MaterialDatePicker: React.FC<MaterialDatePickerProps> = ({
  value,
  onChange,
  minDate,
  maxDate,
}) => {
  const [isOpen, setIsOpen] = useState(false);
  const containerRef = useRef<HTMLDivElement>(null);

  // Parse current selected date
  const selectedDate = value ? new Date(value + 'T00:00:00') : new Date();

  // Calendar view state (month and year we are viewing)
  const [viewMonth, setViewMonth] = useState(selectedDate.getMonth());
  const [viewYear, setViewYear] = useState(selectedDate.getFullYear());

  // Reset view month/year when selectedDate changes or when opened
  useEffect(() => {
    if (isOpen) {
      setViewMonth(selectedDate.getMonth());
      setViewYear(selectedDate.getFullYear());
    }
  }, [isOpen, value]);

  // Click outside and Escape key handler
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (containerRef.current && !containerRef.current.contains(event.target as Node)) {
        setIsOpen(false);
      }
    };
    const handleKeyDown = (event: KeyboardEvent) => {
      if (event.key === 'Escape' && isOpen) {
        setIsOpen(false);
      }
    };
    document.addEventListener('mousedown', handleClickOutside);
    window.addEventListener('keydown', handleKeyDown);
    return () => {
      document.removeEventListener('mousedown', handleClickOutside);
      window.removeEventListener('keydown', handleKeyDown);
    };
  }, [isOpen]);

  const monthNames = [
    'January', 'February', 'March', 'April', 'May', 'June',
    'July', 'August', 'September', 'October', 'November', 'December'
  ];

  const daysOfWeek = ['M', 'T', 'W', 'T', 'F', 'S', 'S'];

  // Date formatters
  const formatDateString = (d: Date) => {
    const year = d.getFullYear();
    const month = String(d.getMonth() + 1).padStart(2, '0');
    const day = String(d.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  };

  const formatDisplayDate = (dateStr: string) => {
    if (!dateStr) return '';
    const parts = dateStr.split('-');
    if (parts.length !== 3) return dateStr;
    return `${parts[2]}/${parts[1]}/${parts[0]}`;
  };

  const handlePrevMonth = () => {
    if (viewMonth === 0) {
      setViewMonth(11);
      setViewYear(prev => prev - 1);
    } else {
      setViewMonth(prev => prev - 1);
    }
  };

  const handleNextMonth = () => {
    if (viewMonth === 11) {
      setViewMonth(0);
      setViewYear(prev => prev + 1);
    } else {
      setViewMonth(prev => prev + 1);
    }
  };

  // Generate calendar grid
  const generateGrid = () => {
    const firstDayOfMonth = new Date(viewYear, viewMonth, 1);
    // JS getDay() returns 0 for Sunday, 1 for Monday, etc.
    const startDayOfWeek = firstDayOfMonth.getDay();
    // Adjust start index to make Monday = 0, Sunday = 6
    const startOffset = startDayOfWeek === 0 ? 6 : startDayOfWeek - 1;

    const daysInMonth = new Date(viewYear, viewMonth + 1, 0).getDate();
    const prevMonthDays = new Date(viewYear, viewMonth, 0).getDate();

    const grid = [];

    // Previous month's trailing days
    for (let i = startOffset - 1; i >= 0; i--) {
      const dayVal = prevMonthDays - i;
      const date = new Date(viewMonth === 0 ? viewYear - 1 : viewYear, viewMonth === 0 ? 11 : viewMonth - 1, dayVal);
      grid.push({ date, isCurrentMonth: false });
    }

    // Current month's days
    for (let i = 1; i <= daysInMonth; i++) {
      const date = new Date(viewYear, viewMonth, i);
      grid.push({ date, isCurrentMonth: true });
    }

    // Next month's leading days to fill standard 42 cell grid
    const remainingCells = 42 - grid.length;
    for (let i = 1; i <= remainingCells; i++) {
      const date = new Date(viewMonth === 11 ? viewYear + 1 : viewYear, viewMonth === 11 ? 0 : viewMonth + 1, i);
      grid.push({ date, isCurrentMonth: false });
    }

    return grid;
  };

  const isDateDisabled = (date: Date) => {
    // Strip time for clean comparison
    const compareDate = new Date(date.getFullYear(), date.getMonth(), date.getDate());
    const minCompare = new Date(minDate.getFullYear(), minDate.getMonth(), minDate.getDate());
    const maxCompare = new Date(maxDate.getFullYear(), maxDate.getMonth(), maxDate.getDate());

    return compareDate < minCompare || compareDate > maxCompare;
  };

  const isDateSelected = (date: Date) => {
    return date.getFullYear() === selectedDate.getFullYear() &&
      date.getMonth() === selectedDate.getMonth() &&
      date.getDate() === selectedDate.getDate();
  };

  const handleDateClick = (date: Date) => {
    if (isDateDisabled(date)) return;
    onChange(formatDateString(date));
    setIsOpen(false);
  };

  const handleTodayClick = () => {
    const today = new Date();
    if (!isDateDisabled(today)) {
      onChange(formatDateString(today));
      setViewMonth(today.getMonth());
      setViewYear(today.getFullYear());
    }
    setIsOpen(false);
  };

  const handleClearClick = () => {
    // Select the maximum allowed date (usually today)
    onChange(formatDateString(maxDate));
    setViewMonth(maxDate.getMonth());
    setViewYear(maxDate.getFullYear());
    setIsOpen(false);
  };

  const gridDays = generateGrid();

  return (
    <div className="relative" ref={containerRef}>
      {/* Date Picker Trigger Button */}
      <button
        type="button"
        aria-haspopup="dialog"
        aria-expanded={isOpen}
        aria-label={`Select date. Current selection is ${formatDisplayDate(value)}`}
        onClick={() => setIsOpen(!isOpen)}
        className="flex items-center gap-3 bg-slate-800 hover:bg-slate-700/80 text-white p-2.5 px-4 rounded-xl border border-slate-700 shadow-md transition-all active:scale-[0.98] select-none focus-visible:ring-2 focus-visible:ring-yellow-400 focus-visible:outline-none"
      >
        <i className="far fa-calendar text-yellow-400 text-sm" aria-hidden="true"></i>
        <span className="text-sm font-bold tracking-wide">{formatDisplayDate(value)}</span>
        <i className={`fas fa-chevron-down text-slate-400 text-xs transition-transform duration-200 ${isOpen ? 'rotate-180' : ''}`} aria-hidden="true"></i>
      </button>

      {/* Calendar Dropdown */}
      {isOpen && (
        <div
          role="dialog"
          aria-label="Calendar date picker"
          aria-modal="false"
          className="absolute right-0 top-full mt-2 w-80 bg-[#1e293b] border border-slate-700 rounded-2xl shadow-2xl p-4 z-50 animate-in fade-in slide-in-from-top-2 duration-150"
        >
          {/* Header */}
          <div className="flex items-center justify-between mb-4">
            <div className="flex items-center gap-1">
              <span className="text-white font-bold text-base" aria-live="polite">
                {monthNames[viewMonth]} {viewYear}
              </span>
            </div>
            <div className="flex items-center gap-3">
              <button
                type="button"
                onClick={handlePrevMonth}
                aria-label="Previous month"
                className="w-8 h-8 flex items-center justify-center rounded-lg hover:bg-slate-700 text-slate-300 hover:text-white transition-all active:scale-95 focus-visible:ring-2 focus-visible:ring-yellow-400 focus-visible:outline-none"
              >
                <i className="fas fa-chevron-left text-sm" aria-hidden="true"></i>
              </button>
              <button
                type="button"
                onClick={handleNextMonth}
                aria-label="Next month"
                className="w-8 h-8 flex items-center justify-center rounded-lg hover:bg-slate-700 text-slate-300 hover:text-white transition-all active:scale-95 focus-visible:ring-2 focus-visible:ring-yellow-400 focus-visible:outline-none"
              >
                <i className="fas fa-chevron-right text-sm" aria-hidden="true"></i>
              </button>
            </div>
          </div>

          {/* Weekday labels */}
          <div className="grid grid-cols-7 gap-1 text-center mb-2" aria-hidden="true">
            {daysOfWeek.map((day, idx) => (
              <span key={idx} className="text-xs font-semibold text-slate-400 py-1">
                {day}
              </span>
            ))}
          </div>

          {/* Days Grid */}
          <div className="grid grid-cols-7 gap-1 text-center" role="grid">
            {gridDays.map(({ date, isCurrentMonth }, idx) => {
              const disabled = isDateDisabled(date);
              const selected = isDateSelected(date);
              
              let textColor = 'text-white font-medium';
              if (!isCurrentMonth) {
                textColor = 'text-slate-500 font-normal';
              }
              if (disabled) {
                textColor = 'text-slate-600 cursor-not-allowed font-normal';
              }

              const formattedDateLabel = `${date.getDate()} ${monthNames[date.getMonth()]} ${date.getFullYear()}${selected ? ', selected' : ''}${disabled ? ', disabled' : ''}`;

              return (
                <button
                  key={idx}
                  type="button"
                  disabled={disabled}
                  aria-label={formattedDateLabel}
                  aria-selected={selected}
                  onClick={() => handleDateClick(date)}
                  className={`
                    w-9 h-9 mx-auto flex items-center justify-center rounded-xl text-sm transition-all relative focus-visible:ring-2 focus-visible:ring-yellow-400 focus-visible:outline-none
                    ${disabled ? 'pointer-events-none opacity-40' : 'hover:bg-slate-700/60 cursor-pointer'}
                    ${selected ? 'bg-blue-500 text-white font-black hover:bg-blue-600 shadow-md shadow-blue-500/20 ring-1 ring-blue-300' : ''}
                    ${textColor}
                  `}
                >
                  {date.getDate()}
                </button>
              );
            })}
          </div>

          {/* Bottom Bar Buttons */}
          <div className="flex items-center justify-between border-t border-slate-800 mt-4 pt-3 text-sm">
            <button
              type="button"
              onClick={handleClearClick}
              className="text-blue-400 hover:text-blue-300 font-bold transition-all px-2 py-1 rounded hover:bg-blue-500/10 active:scale-95 focus-visible:ring-2 focus-visible:ring-yellow-400 focus-visible:outline-none"
            >
              Clear
            </button>
            <button
              type="button"
              onClick={handleTodayClick}
              className="text-blue-400 hover:text-blue-300 font-bold transition-all px-2 py-1 rounded hover:bg-blue-500/10 active:scale-95 focus-visible:ring-2 focus-visible:ring-yellow-400 focus-visible:outline-none"
            >
              Today
            </button>
          </div>
        </div>
      )}
    </div>
  );
};

export default MaterialDatePicker;

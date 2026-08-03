import { forwardRef } from "react";

const Input = forwardRef(
  (
    { label, error, required = false, startIcon, className = "", ...props },
    ref,
  ) => {
    return (
      <div className="space-y-2">
        {label && (
          <label className="flex items-center gap-1 text-sm font-medium text-slate-700">
            {label}

            {required && <span className="text-red-500">*</span>}
          </label>
        )}

        <div className="relative">
          {startIcon && (
            <div className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400">
              {startIcon}
            </div>
          )}

          <input
            ref={ref}
            className={`
              h-11
              w-full
              rounded-lg
              border
              border-slate-300
              bg-white
              px-4
              text-sm
              outline-none
              transition
              placeholder:text-slate-400
              focus:border-blue-500
              focus:ring-4
              focus:ring-blue-100
              disabled:cursor-not-allowed
              disabled:bg-slate-100
              disabled:text-slate-500
              ${startIcon ? "pl-10" : ""}
              ${error ? "border-red-500 focus:ring-red-100" : ""}
              ${className}
            `}
            {...props}
          />
        </div>

        {error && <p className="text-sm text-red-500">{error}</p>}
      </div>
    );
  },
);

Input.displayName = "Input";

export default Input;

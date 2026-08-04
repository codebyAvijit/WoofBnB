import { forwardRef, useState } from "react";

function Input(
  {
    label,
    required = false,
    error,
    helperText,
    leftIcon,
    rightIcon,
    type = "text",
    className = "",
    ...props
  },
  ref,
) {
  const [showPassword, setShowPassword] = useState(false);

  const inputType =
    type === "password" ? (showPassword ? "text" : "password") : type;

  return (
    <div className="space-y-2">
      {label && (
        <label className="flex items-center gap-1 text-sm font-medium text-slate-700">
          {label}

          {required && <span className="text-red-500">*</span>}
        </label>
      )}

      <div className="relative">
        {leftIcon && (
          <div className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400">
            {leftIcon}
          </div>
        )}

        <input
          ref={ref}
          type={inputType}
          className={`
            w-full
            rounded-lg
            border
            px-4
            py-2.5
            transition
            outline-none
            focus:border-blue-500
            focus:ring-4
            focus:ring-blue-100

            ${leftIcon ? "pl-10" : ""}
            ${type === "password" ? "pr-12" : rightIcon ? "pr-10" : ""}
            ${error ? "border-red-500" : "border-slate-300"}

            ${className}
          `}
          {...props}
        />

        {type === "password" ? (
          <button
            type="button"
            onClick={() => setShowPassword((prev) => !prev)}
            className="absolute right-3 top-1/2 -translate-y-1/2 text-sm text-slate-500"
          >
            {showPassword ? "Hide" : "Show"}
          </button>
        ) : (
          rightIcon && (
            <div className="absolute right-3 top-1/2 -translate-y-1/2 text-slate-400">
              {rightIcon}
            </div>
          )
        )}
      </div>

      {helperText && !error && (
        <p className="text-xs text-slate-500">{helperText}</p>
      )}

      {error && <p className="text-sm text-red-500">{error}</p>}
    </div>
  );
}

export default forwardRef(Input);

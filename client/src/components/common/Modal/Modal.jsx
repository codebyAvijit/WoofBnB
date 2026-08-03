function Modal({ isOpen, title, children, onClose, footer }) {
  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4">
      <div className="w-full max-w-lg rounded-xl bg-white shadow-xl">
        <div className="flex items-center justify-between border-b p-5">
          <h2 className="text-xl font-semibold text-slate-800">{title}</h2>

          <button
            onClick={onClose}
            className="text-2xl leading-none text-slate-500 hover:text-slate-800"
          >
            &times;
          </button>
        </div>

        <div className="p-5">{children}</div>

        {footer && <div className="border-t p-5">{footer}</div>}
      </div>
    </div>
  );
}

export default Modal;

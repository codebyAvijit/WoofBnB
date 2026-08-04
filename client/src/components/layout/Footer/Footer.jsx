function Footer() {
  return (
    <footer className="border-t bg-white">
      <div className="mx-auto flex max-w-7xl items-center justify-between px-6 py-8">
        <p className="text-sm text-slate-500">
          © {new Date().getFullYear()} WoofBnB. All rights reserved.
        </p>

        <p className="text-sm text-slate-400">Built with React</p>
      </div>
    </footer>
  );
}

export default Footer;

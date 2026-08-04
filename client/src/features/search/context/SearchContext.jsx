import { createContext, useContext, useState } from "react";

export const SearchContext = createContext(null);

function SearchProvider({ children }) {
  const [searchText, setSearchText] = useState("");
  const [coordinates, setCoordinates] = useState(null);
  const [radius, setRadius] = useState(5000);
  const [searchParams, setSearchParams] = useState(null);

  return (
    <SearchContext.Provider
      value={{
        searchText,
        setSearchText,

        coordinates,
        setCoordinates,

        radius,
        setRadius,

        searchParams,
        setSearchParams,
      }}
    >
      {children}
    </SearchContext.Provider>
  );
}

function useSearch() {
  const context = useContext(SearchContext);

  if (!context) {
    throw new Error("useSearch must be used within SearchProvider");
  }

  return context;
}

export { SearchProvider, useSearch };

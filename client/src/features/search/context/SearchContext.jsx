import { createContext, useContext, useState } from "react";

export const SearchContext = createContext(null);

function SearchProvider({ children }) {
  const [searchText, setSearchText] = useState("");
  const [coordinates, setCoordinates] = useState(null);
  const [radius, setRadius] = useState(15000);
  const [searchParams, setSearchParams] = useState(null);
  const [selectedPetSitter, setSelectedPetSitter] = useState(null);
  const [nearbyPetSitters, setNearbyPetSitters] = useState([]);

  return (
    <SearchContext.Provider
      value={{
        coordinates,
        setCoordinates,

        radius,
        setRadius,

        searchParams,
        setSearchParams,

        nearbyPetSitters,
        setNearbyPetSitters,

        selectedPetSitter,
        setSelectedPetSitter,
        searchText,
        setSearchText,
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

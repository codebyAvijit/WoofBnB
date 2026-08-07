import { toast } from "react-toastify";

import { useSearch } from "../context/SearchContext";

function useLocationSearch() {
  const { coordinates, radius, setSearchParams, setSelectedPetSitter } =
    useSearch();

  const searchByLocation = () => {
    if (!coordinates) {
      toast.warning("Please select a location from the suggestions.");
      return;
    }

    setSelectedPetSitter(null);

    setSearchParams({
      lat: coordinates.latitude,
      lng: coordinates.longitude,
      radius,
    });

    toast.success("Showing nearby pet sitters.");
  };

  return {
    searchByLocation,
  };
}

export default useLocationSearch;

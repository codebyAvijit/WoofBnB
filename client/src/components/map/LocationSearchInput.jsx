import { useRef } from "react";
import { Autocomplete } from "@react-google-maps/api";

import { useSearch } from "../../features/search/context/SearchContext";

function LocationSearchInput() {
  const autocompleteRef = useRef(null);

  const {
    setCoordinates,
    setSearchParams,
    setSearchText,
    radius,
    setSelectedPetSitter,
  } = useSearch();

  const handleLoad = (autocomplete) => {
    autocompleteRef.current = autocomplete;
  };

  const handlePlaceChanged = () => {
    const place = autocompleteRef.current?.getPlace();

    if (!place || !place.geometry || !place.geometry.location) {
      return;
    }

    const latitude = place.geometry.location.lat();
    const longitude = place.geometry.location.lng();

    // Show the selected place in the input
    setSearchText(place.formatted_address || place.name);

    // Store the coordinates
    setCoordinates({
      latitude,
      longitude,
    });

    // Clear any previously selected pet sitter
    setSelectedPetSitter(null);

    // DO NOT call setSearchParams here.
    // The Search button will trigger the nearby search.
  };
  return (
    <Autocomplete onLoad={handleLoad} onPlaceChanged={handlePlaceChanged}>
      <input
        type="text"
        placeholder="Search city or locality..."
        className="w-full rounded-xl border border-slate-300 px-4 py-3 outline-none focus:border-blue-500"
      />
    </Autocomplete>
  );
}

export default LocationSearchInput;

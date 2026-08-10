import { Marker } from "@react-google-maps/api";

import { useSearch } from "../../features/search/context/SearchContext";

function UserMarker() {
  const { coordinates } = useSearch();
  if (!coordinates) return null;

  return (
    <Marker
      position={{
        lat: coordinates.latitude,
        lng: coordinates.longitude,
      }}
      title="Your Location"
    />
  );
}

export default UserMarker;

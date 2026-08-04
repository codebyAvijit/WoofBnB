import { Marker, Popup } from "react-leaflet";
import { useSearch } from "../../features/search/context/SearchContext";

function UserMarker() {
  const { coordinates } = useSearch();

  if (!coordinates) {
    return null;
  }

  return (
    <Marker position={[coordinates.latitude, coordinates.longitude]}>
      <Popup>📍 You are here</Popup>
    </Marker>
  );
}

export default UserMarker;

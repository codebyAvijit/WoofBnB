import { MapContainer as LeafletMap, TileLayer } from "react-leaflet";
import UserMarker from "./UserMarker";
import MapRecenter from "./MapRecenter";
import PetSitterMarker from "./PetSitterMarker";
import { useSearch } from "../../features/search/context/SearchContext";
function MapContainer() {
  const { coordinates, selectedPetSitter } = useSearch();

  return (
    <div className="sticky top-6 h-[700px] overflow-hidden rounded-2xl shadow-lg">
      <LeafletMap
        center={[28.6139, 77.209]}
        zoom={13}
        scrollWheelZoom
        className="h-full w-full"
      >
        <TileLayer
          attribution="&copy; OpenStreetMap contributors"
          url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
        />

        <UserMarker />

        <PetSitterMarker />

        <MapRecenter
          coordinates={coordinates}
          selectedPetSitter={selectedPetSitter}
        />
      </LeafletMap>
    </div>
  );
}

export default MapContainer;

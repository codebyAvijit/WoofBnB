import { useMemo, useCallback, useRef } from "react";
import { GoogleMap } from "@react-google-maps/api";

import { useSearch } from "../../features/search/context/SearchContext";

import UserMarker from "./UserMarker";
import PetSitterMarkers from "./PetSitterMarker";
import MapInfoWindow from "./MapInfoWindow";
const containerStyle = {
  width: "100%",
  height: "100%",
};

const defaultCenter = {
  lat: 28.6139,
  lng: 77.209,
};

function MapContainer() {
  const mapRef = useRef(null);

  const { coordinates, selectedPetSitter } = useSearch();

  const handleMapLoad = useCallback((map) => {
    mapRef.current = map;
  }, []);

  const mapOptions = useMemo(
    () => ({
      streetViewControl: false,
      fullscreenControl: false,
      mapTypeControl: false,
      clickableIcons: false,
    }),
    [],
  );

  const center = useMemo(() => {
    if (selectedPetSitter) {
      return {
        lat: selectedPetSitter.location.coordinates[1],
        lng: selectedPetSitter.location.coordinates[0],
      };
    }

    if (coordinates) {
      return {
        lat: coordinates.latitude,
        lng: coordinates.longitude,
      };
    }

    return defaultCenter;
  }, [coordinates, selectedPetSitter]);

  const zoom = useMemo(() => {
    if (selectedPetSitter) return 15;

    if (coordinates) return 14;

    return 13;
  }, [coordinates, selectedPetSitter]);

  return (
    <div className="sticky top-6 h-[700px] overflow-hidden rounded-2xl shadow-lg">
      <GoogleMap
        mapContainerStyle={containerStyle}
        center={center}
        zoom={zoom}
        options={mapOptions}
        onLoad={handleMapLoad}
      >
        <UserMarker />

        <PetSitterMarkers />
        <MapInfoWindow />
      </GoogleMap>
    </div>
  );
}

export default MapContainer;

import { Marker, Popup } from "react-leaflet";

import { useSearch } from "../../features/search/context/SearchContext";

import MapPopup from "./MapPopup";

function PetSitterMarker() {
  const { nearbyPetSitters, setSelectedPetSitter } = useSearch();
  if (!nearbyPetSitters.length) {
    return null;
  }

  return (
    <>
      {nearbyPetSitters.map((petSitter) => (
        <Marker
          key={petSitter.id}
          position={[
            petSitter.location.coordinates[1],
            petSitter.location.coordinates[0],
          ]}
          eventHandlers={{
            click: () => setSelectedPetSitter(petSitter),
          }}
        >
          <Popup>
            <MapPopup petSitter={petSitter} />
          </Popup>
        </Marker>
      ))}
    </>
  );
}

export default PetSitterMarker;

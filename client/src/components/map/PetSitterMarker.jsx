import { Marker } from "@react-google-maps/api";

import { useSearch } from "../../features/search/context/SearchContext";

function PetSitterMarkers() {
  const { nearbyPetSitters, setSelectedPetSitter } = useSearch();

  if (!nearbyPetSitters.length) return null;

  return (
    <>
      {nearbyPetSitters.map((petSitter) => (
        <Marker
          key={petSitter.id}
          position={{
            lat: petSitter.location.coordinates[1],
            lng: petSitter.location.coordinates[0],
          }}
          title={petSitter.name}
          onClick={() => setSelectedPetSitter(petSitter)}
        />
      ))}
    </>
  );
}

export default PetSitterMarkers;

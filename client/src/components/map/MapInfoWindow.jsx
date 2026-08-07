import { InfoWindow } from "@react-google-maps/api";

import { useSearch } from "../../features/search/context/SearchContext";

function MapInfoWindow() {
  const { selectedPetSitter, setSelectedPetSitter } = useSearch();

  if (!selectedPetSitter) return null;

  return (
    <InfoWindow
      position={{
        lat: selectedPetSitter.location.coordinates[1],
        lng: selectedPetSitter.location.coordinates[0],
      }}
      onCloseClick={() => setSelectedPetSitter(null)}
    >
      <div className="min-w-[250px] space-y-2">
        <h3 className="text-lg font-semibold">{selectedPetSitter.name}</h3>

        <p className="text-sm text-slate-500">{selectedPetSitter.address}</p>

        <div className="text-sm">
          <p>📞 {selectedPetSitter.phone}</p>

          <p>
            🕘 {selectedPetSitter.workingHours.start}
            {" - "}
            {selectedPetSitter.workingHours.end}
          </p>
        </div>

        <div className="flex flex-wrap gap-2">
          {selectedPetSitter.amenities.map((amenity) => (
            <span
              key={amenity}
              className="rounded-full bg-blue-100 px-2 py-1 text-xs text-blue-700"
            >
              {amenity}
            </span>
          ))}
        </div>
      </div>
    </InfoWindow>
  );
}

export default MapInfoWindow;

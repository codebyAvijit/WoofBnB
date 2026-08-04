import Loader from "../common/Loader";

import PetSitterCard from "../../features/petsitter/components/PetSitterCard";

import useNearbyPetSitters from "../../features/petsitter/hooks/useNearbyPetSitters";
import { useSearch } from "../../features/search/context/SearchContext";

function NearbyList() {
  const { searchParams, selectedPetSitter, setSelectedPetSitter } = useSearch();

  const {
    data: petSitters = [],
    isLoading,
    isError,
    error,
  } = useNearbyPetSitters(searchParams);

  return (
    <section className="bg-slate-100 py-20">
      <div className="mx-auto max-w-7xl px-6">
        <h2 className="mb-10 text-4xl font-bold text-slate-900">
          Nearby Pet Sitters
        </h2>

        {!searchParams && (
          <p className="text-slate-500">
            Search using your location to find nearby pet sitters.
          </p>
        )}

        {isLoading && (
          <div className="flex justify-center py-10">
            <Loader />
          </div>
        )}

        {isError && (
          <p className="text-red-500">
            {error?.response?.data?.message ??
              "Unable to fetch nearby pet sitters."}
          </p>
        )}

        {!isLoading && !isError && searchParams && petSitters.length === 0 && (
          <p className="text-slate-500">
            No pet sitters found in your selected area.
          </p>
        )}

        {!isLoading && !isError && petSitters.length > 0 && (
          <div className="h-[700px] space-y-4 overflow-y-auto pr-2">
            {petSitters.map((petSitter) => (
              <div
                key={petSitter.id}
                onClick={() => setSelectedPetSitter(petSitter)}
                className={`
cursor-pointer
rounded-xl
border
transition-all
duration-200

${
  selectedPetSitter?.id === petSitter.id
    ? "border-blue-600 bg-blue-50 shadow-lg"
    : "border-slate-200 hover:border-blue-300 hover:shadow-md"
}
`}
              >
                <PetSitterCard petSitter={petSitter} />
              </div>
            ))}
          </div>
        )}
      </div>
    </section>
  );
}

export default NearbyList;

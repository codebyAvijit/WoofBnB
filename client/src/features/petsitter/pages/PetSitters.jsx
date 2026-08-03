import Loader from "../../../components/common/Loader";

import PetSitterCard from "../components/PetSitterCard";

import usePetSitters from "../hooks/usePetsitters";

function PetSitters() {
  const { data: petSitters, isLoading, isError } = usePetSitters();

  if (isLoading) {
    return <Loader />;
  }

  if (isError) {
    return <p className="text-red-500">Failed to load pet sitters.</p>;
  }

  return (
    <>
      <div className="space-y-6">
        <div className="flex items-center justify-between">
          <h1 className="text-3xl font-bold">Pet Sitters</h1>
        </div>

        <div className="grid gap-6 md:grid-cols-2 xl:grid-cols-3">
          {petSitters?.map((petSitter) => (
            <PetSitterCard key={petSitter.id} petSitter={petSitter} />
          ))}
        </div>
      </div>
    </>
  );
}

export default PetSitters;

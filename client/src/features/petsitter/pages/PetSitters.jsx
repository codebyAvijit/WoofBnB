import { useState } from "react";

import Button from "../../../components/common/Button";
import Loader from "../../../components/common/Loader";
import Modal from "../../../components/common/Modal";

import PetSitterCard from "../components/PetSitterCard";
import PetSitterForm from "../components/PetSitterForm";

import usePetSitters from "../hooks/usePetsitters";

function PetSitters() {
  const [openModal, setOpenModal] = useState(false);

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

          <Button onClick={() => setOpenModal(true)}>+ Add Pet Sitter</Button>
        </div>

        <div className="grid gap-6 md:grid-cols-2 xl:grid-cols-3">
          {petSitters?.map((petSitter) => (
            <PetSitterCard key={petSitter.id} petSitter={petSitter} />
          ))}
        </div>
      </div>

      <Modal
        isOpen={openModal}
        title="Create Pet Sitter"
        onClose={() => setOpenModal(false)}
      >
        <PetSitterForm onSuccess={() => setOpenModal(false)} />
      </Modal>
    </>
  );
}

export default PetSitters;

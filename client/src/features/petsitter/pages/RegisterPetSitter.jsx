import { useNavigate } from "react-router-dom";
import { toast } from "react-toastify";

import Card from "../../../components/common/Card";

import PetSitterForm from "../components/PetSitterForm";

function RegisterPetSitter() {
  const navigate = useNavigate();

  return (
    <div className="flex min-h-screen items-center justify-center bg-slate-100 p-6">
      <Card className="w-full max-w-3xl">
        <h1 className="mb-6 text-3xl font-bold">Become a Pet Sitter</h1>

        <PetSitterForm
          onSuccess={() => {
            navigate("/");
          }}
        />
      </Card>
    </div>
  );
}

export default RegisterPetSitter;

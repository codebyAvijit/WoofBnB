import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "react-toastify";

import { createPetSitter } from "../../../api/petsitter.api";

function useCreatePetSitter(onSuccess) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: createPetSitter,

    onSuccess: ({ data }) => {
      toast.success(data.message);

      queryClient.invalidateQueries({
        queryKey: ["petsitters"],
      });

      onSuccess?.();
    },

    onError: (error) => {
      toast.error(
        error?.response?.data?.message ?? "Unable to create pet sitter.",
      );
    },
  });
}

export default useCreatePetSitter;

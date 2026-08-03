import { useQuery } from "@tanstack/react-query";

import { getPetSitters } from "../../../api/petsitter.api";

function usePetSitters() {
  return useQuery({
    queryKey: ["petsitters"],

    queryFn: async () => {
      const { data } = await getPetSitters();

      return data.data;
    },
  });
}

export default usePetSitters;

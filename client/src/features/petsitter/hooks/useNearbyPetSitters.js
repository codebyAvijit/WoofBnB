import { useQuery } from "@tanstack/react-query";

import { getNearbyPetSitters } from "../../../api/petsitter.api";

function useNearbyPetSitters(searchParams) {
  return useQuery({
    queryKey: ["nearby-petsitters", searchParams],

    queryFn: async () => {
      const { data } = await getNearbyPetSitters(searchParams);

      return data.data;
    },

    enabled: Boolean(searchParams),
  });
}

export default useNearbyPetSitters;

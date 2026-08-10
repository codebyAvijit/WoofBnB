import { useEffect } from "react";
import { useQuery } from "@tanstack/react-query";

import { getNearbyPetSitters } from "../../../api/petsitter.api";
import { useSearch } from "../../search/context/SearchContext";

function useNearbyPetSitters(searchParams) {
  const { setNearbyPetSitters, setSelectedPetSitter } = useSearch();
  const query = useQuery({
    queryKey: ["nearby-petsitters", searchParams],

    queryFn: async () => {
      const { data } = await getNearbyPetSitters(searchParams);
      return data.data;
    },

    enabled: Boolean(searchParams),
  });

  useEffect(() => {
    if (!query.data) {
      setNearbyPetSitters([]);
      setSelectedPetSitter(null);
      return;
    }

    setNearbyPetSitters(query.data);

    if (query.data.length === 0) {
      setSelectedPetSitter(null);
    }
  }, [query.data, setNearbyPetSitters, setSelectedPetSitter]);

  return query;
}

export default useNearbyPetSitters;

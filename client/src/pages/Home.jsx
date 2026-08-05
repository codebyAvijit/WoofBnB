import { useEffect } from "react";

import HeroSection from "../components/home/HeroSection";
import SearchSection from "../components/home/SearchSection";
import NearbyExplorer from "../components/home/NearbyExplorer";

import useCurrentLocation from "../features/petsitter/hooks/useCurrentLocation";

function Home() {
  const { getCurrentLocation } = useCurrentLocation();

  useEffect(() => {
    getCurrentLocation();
  }, [getCurrentLocation]);

  return (
    <>
      <HeroSection />

      <SearchSection />

      <NearbyExplorer />
    </>
  );
}

export default Home;

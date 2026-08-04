import { useState } from "react";

import { useSearch } from "../../search/context/SearchContext";

function useCurrentLocation() {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  const { setCoordinates, setSearchParams, radius } = useSearch();

  const getCurrentLocation = () => {
    if (loading) return;

    if (!navigator.geolocation) {
      setError("Geolocation is not supported by this browser.");
      return;
    }

    setLoading(true);
    setError("");

    navigator.geolocation.getCurrentPosition(
      ({ coords: { latitude, longitude } }) => {
        setCoordinates({
          latitude,
          longitude,
        });

        setSearchParams({
          lat: latitude,
          lng: longitude,
          radius,
        });

        console.log("Current Location:", {
          latitude,
          longitude,
        });

        setLoading(false);
      },
      (geoError) => {
        switch (geoError.code) {
          case geoError.PERMISSION_DENIED:
            setError("Location permission was denied.");
            break;

          case geoError.POSITION_UNAVAILABLE:
            setError("Location information is unavailable.");
            break;

          case geoError.TIMEOUT:
            setError("Location request timed out.");
            break;

          default:
            setError("Unable to fetch your current location.");
        }

        setLoading(false);
      },
      {
        enableHighAccuracy: true,
        timeout: 10000,
        maximumAge: 0,
      },
    );
  };

  return {
    loading,
    error,
    getCurrentLocation,
  };
}

export default useCurrentLocation;

import { useEffect } from "react";
import { useMap } from "react-leaflet";

function MapRecenter({ coordinates, selectedPetSitter }) {
  const map = useMap();

  useEffect(() => {
    if (selectedPetSitter) {
      const [lng, lat] = selectedPetSitter.location.coordinates;

      map.flyTo([lat, lng], 15, {
        animate: true,
        duration: 1.5,
      });

      return;
    }

    if (coordinates) {
      map.flyTo([coordinates.latitude, coordinates.longitude], 14, {
        animate: true,
        duration: 1.5,
      });
    }
  }, [coordinates, selectedPetSitter, map]);

  return null;
}

export default MapRecenter;

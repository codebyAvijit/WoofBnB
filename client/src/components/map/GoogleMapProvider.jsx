import { LoadScript } from "@react-google-maps/api";

function GoogleMapProvider({ children }) {
  return (
    <LoadScript
      googleMapsApiKey={import.meta.env.VITE_GOOGLE_MAPS_API_KEY}
      libraries={["places"]}
    >
      {children}
    </LoadScript>
  );
}

export default GoogleMapProvider;

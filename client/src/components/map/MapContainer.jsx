import { GoogleMap } from "@react-google-maps/api";

const containerStyle = {
  width: "100%",
  height: "100%",
};

const center = {
  lat: 28.6139,
  lng: 77.209,
};

function MapContainer() {
  return (
    <div className="sticky top-6 h-[700px] overflow-hidden rounded-2xl shadow-lg">
      <GoogleMap
        mapContainerStyle={containerStyle}
        center={center}
        zoom={13}
        options={{
          streetViewControl: false,
          fullscreenControl: false,
          mapTypeControl: false,
        }}
      />
    </div>
  );
}

export default MapContainer;

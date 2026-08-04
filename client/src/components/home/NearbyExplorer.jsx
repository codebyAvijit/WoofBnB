import NearbyList from "./NearbyList";
import MapContainer from "../map/MapContainer";

function NearbyExplorer() {
  return (
    <section className="bg-slate-100 py-10">
      <div className="mx-auto max-w-7xl px-6">
        <div className="grid gap-6 lg:grid-cols-12">
          <div className="lg:col-span-4">
            <NearbyList />
          </div>

          <div className="lg:col-span-8">
            <MapContainer />
          </div>
        </div>
      </div>
    </section>
  );
}

export default NearbyExplorer;

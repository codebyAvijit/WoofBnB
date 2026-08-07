import { useSearch } from "../../features/search/context/SearchContext";

import Button from "../common/Button";
import useCurrentLocation from "../../features/petsitter/hooks/useCurrentLocation";
import useLocationSearch from "../../features/search/hooks/useLocationSearch";
import LocationSearchInput from "../map/LocationSearchInput";
function SearchSection() {
  const { searchText, setSearchText, radius, coordinates, setSearchParams } =
    useSearch();
  const { searchByLocation } = useLocationSearch();
  const { loading, error, getCurrentLocation } = useCurrentLocation();

  return (
    <section className="bg-white py-16">
      <div className="mx-auto max-w-7xl px-6">
        <div className="rounded-3xl bg-slate-50 p-8 shadow-lg">
          <div className="mx-auto max-w-3xl text-center">
            <h2 className="text-4xl font-bold text-slate-900">
              Find Pet Sitters Near You
            </h2>

            <p className="mt-3 text-slate-500">
              Search by city or use your current location to discover verified
              pet sitters.
            </p>
          </div>

          <div className="mx-auto mt-10 flex max-w-3xl flex-col gap-4 md:flex-row">
            <LocationSearchInput />

            <Button size="lg" onClick={searchByLocation}>
              Search
            </Button>
          </div>

          <div className="mt-6 flex justify-center">
            <Button
              variant="secondary"
              size="lg"
              loading={loading}
              onClick={getCurrentLocation}
            >
              📍 Refresh Current Location
            </Button>
            {error && (
              <p className="mt-4 text-center text-sm text-red-500">{error}</p>
            )}
          </div>
        </div>
      </div>
    </section>
  );
}

export default SearchSection;

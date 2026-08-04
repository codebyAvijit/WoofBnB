import { useSearch } from "../../features/search/context/SearchContext";

import Button from "../common/Button";
import Input from "../common/Input";
import useCurrentLocation from "../../features/petsitter/hooks/useCurrentLocation";

function SearchSection() {
  const { searchText, setSearchText, radius, coordinates, setSearchParams } =
    useSearch();

  const { loading, error, getCurrentLocation } = useCurrentLocation();

  const handleSearch = () => {
    if (!coordinates) return;

    setSearchParams({
      lat: coordinates.latitude,
      lng: coordinates.longitude,
      radius,
    });
  };

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
            <Input
              placeholder="Enter city or locality"
              value={searchText}
              onChange={(e) => setSearchText(e.target.value)}
            />

            <Button size="lg" onClick={handleSearch}>
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
              📍 Use Current Location
            </Button>
            {coordinates && (
              <div className="mt-6 rounded-lg bg-green-50 p-4">
                <p className="text-sm text-green-700">
                  Latitude: {coordinates.latitude}
                </p>

                <p className="text-sm text-green-700">
                  Longitude: {coordinates.longitude}
                </p>
              </div>
            )}
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

function MapPopup({ petSitter }) {
  return (
    <div className="min-w-[260px] space-y-3">
      <div>
        <h3 className="text-lg font-semibold text-slate-800">
          {petSitter.name}
        </h3>

        <p className="text-sm text-slate-500">{petSitter.address}</p>
      </div>

      <div className="space-y-1 text-sm">
        <p>
          <span className="font-medium">📞 Phone:</span> {petSitter.phone}
        </p>

        <p>
          <span className="font-medium">🕒 Working Hours:</span>{" "}
          {petSitter.workingHours.start} - {petSitter.workingHours.end}
        </p>
      </div>

      <div>
        <p className="mb-2 font-medium text-slate-700">Amenities</p>

        <div className="flex flex-wrap gap-2">
          {petSitter.amenities.map((amenity) => (
            <span
              key={amenity}
              className="rounded-full bg-blue-100 px-2 py-1 text-xs text-blue-700"
            >
              {amenity}
            </span>
          ))}
        </div>
      </div>
    </div>
  );
}

export default MapPopup;

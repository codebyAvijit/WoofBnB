import Card from "../../../components/common/Card";

function PetSitterCard({ petSitter }) {
  return (
    <Card className="space-y-3">
      <h2 className="text-xl font-semibold">{petSitter.name}</h2>

      <p>{petSitter.email}</p>

      <p>{petSitter.phone}</p>

      <p>{petSitter.address}</p>

      <div className="flex flex-wrap gap-2">
        {petSitter.amenities.map((item) => (
          <span
            key={item}
            className="rounded bg-blue-100 px-2 py-1 text-xs text-blue-700"
          >
            {item}
          </span>
        ))}
      </div>
    </Card>
  );
}

export default PetSitterCard;

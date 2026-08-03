import Button from "../../../components/common/Button";
import Input from "../../../components/common/Input";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";

import { petSitterSchema } from "../petsitterValidation";
import { PET_SITTER_AMENITIES } from "../../../utils/constants";
import useCreatePetSitter from "../hooks/useCreatePetSitter";
import { mapCreatePetSitterPayload } from "../petsitter.mapper";

function PetSitterForm({ onSuccess }) {
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm({
    resolver: zodResolver(petSitterSchema),

    defaultValues: {
      name: "",
      email: "",
      phone: "",
      address: "",
      bio: "",
      startTime: "",
      endTime: "",
      amenities: [],
    },
  });
  const mutation = useCreatePetSitter(onSuccess);
  const onSubmit = (formData) => {
    const payload = mapCreatePetSitterPayload(formData);

    mutation.mutate(payload);
  };

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
      {/* Basic Information */}
      <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
        <Input
          label="Name"
          required
          placeholder="Enter full name"
          error={errors.name?.message}
          {...register("name")}
        />

        <Input
          label="Email"
          type="email"
          required
          placeholder="Enter email"
          error={errors.email?.message}
          {...register("email")}
        />

        <Input
          label="Phone"
          required
          placeholder="Enter phone number"
          error={errors.phone?.message}
          {...register("phone")}
        />

        <Input
          label="Address"
          required
          placeholder="Enter address"
          error={errors.address?.message}
          {...register("address")}
        />
      </div>

      {/* Bio */}
      <div>
        <label className="mb-2 flex items-center gap-1 text-sm font-medium text-slate-700">
          Bio
          <span className="text-red-500">*</span>
        </label>
        <textarea
          rows={3}
          placeholder="Write a short bio..."
          {...register("bio")}
          className="
    w-full
    rounded-lg
    border
    border-slate-300
    px-4
    py-3
    text-sm
    outline-none
    transition
    placeholder:text-slate-400
    focus:border-blue-500
    focus:ring-4
    focus:ring-blue-100
  "
        />
        {errors.bio && (
          <p className="mt-2 text-sm text-red-500">{errors.bio.message}</p>
        )}
      </div>

      {/* Working Hours */}
      <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
        <Input
          type="time"
          label="Start Time"
          required
          error={errors.startTime?.message}
          {...register("startTime")}
        />
        <Input
          type="time"
          label="End Time"
          required
          error={errors.endTime?.message}
          {...register("endTime")}
        />
      </div>

      {/* Amenities */}
      <div>
        <label className="mb-3 block text-sm font-medium text-slate-700">
          Amenities
        </label>

        <div className="grid grid-cols-2 gap-3 lg:grid-cols-3">
          {PET_SITTER_AMENITIES.map((amenity) => (
            <label
              key={amenity}
              className="flex cursor-pointer items-center gap-2 rounded-md px-2 py-1 transition hover:bg-slate-100"
            >
              <input
                type="checkbox"
                value={amenity}
                {...register("amenities")}
                className="h-4 w-4 accent-blue-600"
              />

              <span className="text-sm text-slate-700">{amenity}</span>
            </label>
          ))}
        </div>
        {errors.amenities && (
          <p className="mt-2 text-sm text-red-500">
            {errors.amenities.message}
          </p>
        )}
      </div>

      {/* Actions */}
      <div className="flex justify-end border-t pt-5">
        <Button type="submit" loading={mutation.isPending}>
          Create Pet Sitter
        </Button>
      </div>
    </form>
  );
}

export default PetSitterForm;

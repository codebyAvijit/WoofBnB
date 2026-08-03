import { useMutation } from "@tanstack/react-query";

import { useNavigate } from "react-router-dom";

import { toast } from "react-toastify";

import { login as loginApi } from "../../../api/auth.api";

import { storage } from "../../../utils/storage";

import { useAuth } from "../../../context/AuthContext";

function useLogin() {
  const navigate = useNavigate();

  const { login } = useAuth();

  return useMutation({
    mutationFn: loginApi,

    onSuccess: ({ data }) => {
      storage.setAccessToken(data.data.accessToken);

      login(data.data.user);

      toast.success(data.message);

      navigate("/", {
        replace: true,
      });
    },

    onError: (error) => {
      toast.error(error?.response?.data?.message ?? "Unable to login.");
    },
  });
}

export default useLogin;

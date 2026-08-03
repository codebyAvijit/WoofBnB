import { useNavigate } from "react-router-dom";
import { toast } from "react-toastify";

import { useAuth } from "../../../context/AuthContext";

function useLogout() {
  const navigate = useNavigate();

  const { logout } = useAuth();

  return () => {
    navigate("/", {
      replace: true,
    });

    logout();

    toast.success("Logged out successfully.");
  };
}

export default useLogout;

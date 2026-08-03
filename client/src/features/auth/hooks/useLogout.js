import { flushSync } from "react-dom";
import { useNavigate } from "react-router-dom";
import { toast } from "react-toastify";

import { useAuth } from "../../../context/AuthContext";

function useLogout() {
  const navigate = useNavigate();

  const { logout } = useAuth();

  return () => {
    flushSync(() => {
      logout();
    });

    toast.success("Logged out successfully.");

    navigate("/", {
      replace: true,
    });
  };
}

export default useLogout;

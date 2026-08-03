import { createContext, useContext, useEffect, useState } from "react";

import { getCurrentUser } from "../api/auth.api";
import { storage } from "../utils/storage";

const AuthContext = createContext(null);

function AuthProvider({ children }) {
  const [user, setUser] = useState(storage.getUser());

  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const initializeAuth = async () => {
      if (!storage.isAuthenticated()) {
        setLoading(false);
        return;
      }

      try {
        const { data } = await getCurrentUser();

        storage.setUser(data.data);

        setUser(data.data);
      } catch {
        storage.clear();

        setUser(null);
      } finally {
        setLoading(false);
      }
    };

    initializeAuth();
  }, []);

  const login = (user) => {
    storage.setUser(user);

    setUser(user);
  };

  const logout = () => {
    storage.clear();

    setUser(null);
  };

  return (
    <AuthContext.Provider
      value={{
        user,
        loading,
        login,
        logout,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
}

export { AuthProvider };

export const useAuth = () => useContext(AuthContext);

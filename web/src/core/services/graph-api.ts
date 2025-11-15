import { apiClient } from "@/core/api/client.browser";

export interface GraphUser {
  id: string;
  displayName: string;
  mail: string | null;
  userPrincipalName: string;
  jobTitle?: string | null;
}

/**
 * Search for users in Azure AD via backend API proxy
 * @param searchQuery - Search term (email, name, or username)
 * @returns List of matching users
 */
export async function searchGraphUsers(searchQuery: string): Promise<GraphUser[]> {
  if (!searchQuery || searchQuery.trim().length < 2) {
    return [];
  }

  try {
    const { data } = await apiClient.request<GraphUser[]>({
      path: "/users/search",
      method: "GET",
      query: { query: searchQuery }
    });

    return data || [];
  } catch (error) {
    console.error("Error searching users:", error);
    return []; // Return empty array on error for better UX
  }
}

/**
 * Get a specific user by ID from Azure AD via backend API proxy
 * @param userId - User GUID
 * @returns User details
 */
export async function getGraphUser(userId: string): Promise<GraphUser | null> {
  try {
    const { data } = await apiClient.request<GraphUser>({
      path: `/users/${userId}`,
      method: "GET"
    });

    return data;
  } catch (error) {
    console.error("Error fetching user:", error);
    return null;
  }
}


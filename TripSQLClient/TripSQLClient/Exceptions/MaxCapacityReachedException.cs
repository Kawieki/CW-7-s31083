namespace TripSQLClient.Exceptions;

public class MaxCapacityReachedException(string message) : Exception(message);
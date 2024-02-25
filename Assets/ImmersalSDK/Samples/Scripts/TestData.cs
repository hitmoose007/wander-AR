using Firebase.Firestore;

[FirestoreData]

public struct Maps
{
    [FirestoreProperty]
    public int mapID { get; set; }

    [FirestoreProperty]
    public string mapName { get; set; }

    [FirestoreProperty]
    public string mapThumbnail { get; set; }
}
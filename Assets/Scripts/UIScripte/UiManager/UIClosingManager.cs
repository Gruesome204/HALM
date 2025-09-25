using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class UIClosingManager : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Find all active UIDocuments
            var allDocs = FindObjectsOfType<UIDocument>().Where(doc => doc.isActiveAndEnabled && doc.sortingOrder != 0);

            if (!allDocs.Any())
                return;

            // Get the top-most one by sortOrder
            var topDoc = allDocs.OrderByDescending(doc => doc.sortingOrder) // shortcut for doc.panelSettings.sortOrder
                .FirstOrDefault();

            if (topDoc != null)
            {
                Debug.Log($"Closing top-most UI: {topDoc.name}");
                CloseUi(topDoc);
            }
        }

    }

    private void CloseUi(UIDocument doc)
    {
        // Your logic to close this specific document
        doc.gameObject.SetActive(false);
    }
}

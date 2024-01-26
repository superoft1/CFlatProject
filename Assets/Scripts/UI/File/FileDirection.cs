using System;
using Chiyoda.CAD.Core;
using Chiyoda.CAD.Model;
using Chiyoda.CAD.Model.Structure;
using Chiyoda.CAD.Plotplan;
using Chiyoda.UI;
using UnityEngine;
using UnityEngine.UI;

public class FileDirection : MonoBehaviour
{
  [SerializeField] private ImportView importView;
  [SerializeField] private ExportView exportView;
  
  public void TestFacilities()
  {
  }

  public void New()
  {
  }

  public void Open()
  {
    if ( null != DocumentCollection.Instance.Current ) DocumentCollection.Instance.Close( DocumentCollection.Instance.Current ) ;
    var doc = DocumentCollection.Instance.CreateNew() ;
    doc.Load(@"E:\gprandrpg\Chiyoda\a.xml") ;
  }

  public void Save()
  {
    DocumentCollection.Instance.Current.Save(@"E:\gprandrpg\Chiyoda\a.xml") ;
  }

  public void Import()
  {
    importView.gameObject.SetActive( true );
  }

  public void Export()
  {
    exportView.gameObject.SetActive( true );
  }

  public void Undo()
  {
    DocumentCollection.Instance.Current.History.Back(1);
  }

  public void Redo()
  {
    DocumentCollection.Instance.Current.History.Go(1);
  }

  #region Target Document

  private Document _document;

  private void SetTargetDocument(Document document)
  {
    if (_document == document) return;

    if (null != _document)
    {
      RemoveDocumentEvents(_document);
    }

    _document = document;

    if (null == document)
    {
      SetCanUndo(false);
      SetCanRedo(false);
    }
    else
    {
      AddDocumentEvents(document);
      SetCanUndo(document.History.CanUndo);
      SetCanRedo(document.History.CanRedo);
    }
  }

  private void AddDocumentEvents(Document document)
  {
    document.History.HistoryStateChanged += Document_HistoryStateChanged;
  }

  private void RemoveDocumentEvents(Document document)
  {
    document.History.HistoryStateChanged -= Document_HistoryStateChanged;
  }

  private void Document_HistoryStateChanged(object sender, EventArgs e)
  {
    var history = sender as History;
    SetCanUndo(history.CanUndo);
    SetCanRedo(history.CanRedo);
  }

  private void SetCanUndo(bool historyCanUndo)
  {
    transform.Find("Undo").GetComponent<Button>().enabled = historyCanUndo;
  }

  private void SetCanRedo(bool historyCanRedo)
  {
    transform.Find("Redo").GetComponent<Button>().enabled = historyCanRedo;
  }

  #endregion

  #region Unity Events

  private void Start()
  {
    SetCanUndo(false);
    SetCanRedo(false);

    DocumentCollection.Instance.CurrentDocumentChanged += DocumentCollection_CurrentDocumentChanged;
    SetTargetDocument(DocumentCollection.Instance.Current);
  }

  private void OnDestroy()
  {
    DocumentCollection.Instance.CurrentDocumentChanged -= DocumentCollection_CurrentDocumentChanged;
    SetTargetDocument(null);
  }

  private void DocumentCollection_CurrentDocumentChanged(object sender, EventArgs e)
  {
    SetTargetDocument(DocumentCollection.Instance.Current);
  }

  #endregion
}
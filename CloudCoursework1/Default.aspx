<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="CloudCoursework1.Default" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Upload Sound</title>
</head>
<body>
    <form id="uploadForm" runat="server">
        <div>
            <h1>Upload Sound</h1>
            <asp:FileUpload ID="soundFileUpload" runat="server" />
            <asp:Button ID="uploadButton" runat="server" Text="Upload" OnClick="UploadButton_Click" />
            <asp:Label ID="messageLabel" runat="server" Text=""></asp:Label>
        </div>

        <div>
            <h2>Uploaded</h2>
            <asp:UpdatePanel ID="updatePanel" runat="server">
                <ContentTemplate>
                    <asp:ListView ID="soundDisplayControl" runat="server" ItemPlaceholderID="soundsPlaceholder">
                        <LayoutTemplate>
                            <asp:PlaceHolder runat="server" ID="soundsPlaceholder"></asp:PlaceHolder>
                        </LayoutTemplate>
                        <ItemTemplate>
                            <audio src='<%# Eval("Url") %>' controls="" preload="none"></audio>
                            <asp:Literal ID="label" Text='<%# Eval("Title") %>' runat="server" /><br />
                        </ItemTemplate>
                    </asp:ListView>

                </ContentTemplate>
            </asp:UpdatePanel>

            <asp:Button ID="RefreshButton" runat="server" Text="Refresh" OnClick="RefreshButton_Click" />
        </div>
    </form>
</body>
</html>

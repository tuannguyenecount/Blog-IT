/**
 * @license Copyright (c) 2003-2015, CKSource - Frederico Knabben. All rights reserved.
 * For licensing, see LICENSE.md or http://ckeditor.com/license
 */

CKEDITOR.editorConfig = function( config ) {
	// Define changes to default configuration here. For example:
	 config.language = 'vi';
    //config.uiColor = '#2d3e50';
	config.height = "500px";
    config.filebrowserBrowseUrl = '/ckfinder/ckfinder.html';

    config.filebrowserImageBrowseUrl = '/ckfinder/ckfinder.html?type=Images';

    config.filebrowserFlashBrowseUrl = '/ckfinder/ckfinder.html?type=Flash';

    config.filebrowserUploadUrl = '/ckfinder/core/connector/aspx/connector.aspx?command=QuickUpload&type=Files';

    config.filebrowserImageUploadUrl = '/ckfinder/core/connector/aspx/connector.aspx?command=QuickUpload&type=Images';

    config.filebrowserFlashUploadUrl = '/ckfinder/core/connector/aspx/connector.aspx?command=QuickUpload&type=Flash';
   
};

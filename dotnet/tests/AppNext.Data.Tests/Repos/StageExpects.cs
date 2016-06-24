namespace AppBoot.Repos
{
    public class StageExpects
    {
        // ReSharper disable InconsistentNaming
        public bool AfterInsert_Found { get; set; }
        public bool AfterInsert_HasChanges { get; set; }
        public bool AfterEdit_HasChanges { get; set; }
        public bool AfterUpdate_HasChanges { get; set; }
        public bool AfterDelete_Found { get; set; }
        public bool AfterDelete_HasChanges { get; set; }
        // ReSharper restore InconsistentNaming

        public static StageExpects DefaultNoSession()
        {
            var expects = new StageExpects();

            expects.AfterInsert_Found = true;
            expects.AfterInsert_HasChanges = false;
            expects.AfterEdit_HasChanges = true;
            expects.AfterUpdate_HasChanges = false;
            expects.AfterDelete_Found = false;
            expects.AfterDelete_HasChanges = false;

            return expects;
        }

        public static StageExpects DefaultUseSession()
        {
            var expects = new StageExpects();

            expects.AfterInsert_Found = true;
            expects.AfterInsert_HasChanges = true;
            expects.AfterEdit_HasChanges = true;
            expects.AfterUpdate_HasChanges = true;
            expects.AfterDelete_Found = false;
            expects.AfterDelete_HasChanges = true;

            return expects;
        }
    }
}
